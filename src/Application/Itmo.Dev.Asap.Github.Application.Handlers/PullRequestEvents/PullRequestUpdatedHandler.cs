using Itmo.Dev.Asap.Github.Application.Core.Models;
using Itmo.Dev.Asap.Github.Application.Core.Services;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Dto.PullRequests;
using Itmo.Dev.Asap.Github.Application.Octokit.Extensions;
using Itmo.Dev.Asap.Github.Application.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Common.Exceptions.Entities;
using Itmo.Dev.Asap.Github.Common.Extensions;
using Itmo.Dev.Asap.Github.Domain.Assignments;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using Itmo.Dev.Asap.Github.Domain.Submissions;
using Itmo.Dev.Asap.Github.Domain.Users;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents.PullRequestUpdated;

namespace Itmo.Dev.Asap.Github.Application.Handlers.PullRequestEvents;

internal class PullRequestUpdatedHandler : IRequestHandler<Command, Response>
{
    private readonly ISubmissionWorkflowService _submissionWorkflowService;
    private readonly IPullRequestEventNotifier _notifier;
    private readonly IPersistenceContext _context;

    public PullRequestUpdatedHandler(
        ISubmissionWorkflowService submissionWorkflowService,
        IPullRequestEventNotifier notifier,
        IPersistenceContext context)
    {
        _submissionWorkflowService = submissionWorkflowService;
        _notifier = notifier;
        _context = context;
    }

    public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(request.PullRequest.SenderId, cancellationToken);

        var studentQuery = GithubSubjectCourseStudentQuery.Build(x => x
            .WithRepositoryId(request.PullRequest.RepositoryId));

        GithubSubjectCourseStudent? student = await _context.SubjectCourses
            .QueryStudentsAsync(studentQuery, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (student is null)
            return new Response.StudentNotFound();

        GithubAssignment? assignment = await _context.Assignments
            .FindAssignmentForPullRequestAsync(request.PullRequest, cancellationToken);

        if (assignment is null)
        {
            string message = await GetSubjectCourseAssignmentsString(request.PullRequest, cancellationToken);

            throw EntityNotFoundException.AssignmentWasNotFound(
                request.PullRequest.BranchName,
                request.PullRequest.OrganizationName,
                message);
        }

        SubmissionUpdateResult result = await _submissionWorkflowService.SubmissionUpdatedAsync(
            issuer.Id,
            student.User.Id,
            assignment.Id,
            request.PullRequest.Payload,
            cancellationToken);

        if (result.IsCreated)
        {
            var submission = new GithubSubmission(
                result.Submission.Id,
                assignment.Id,
                student.User.Id,
                result.Submission.SubmissionDate,
                request.PullRequest.OrganizationId,
                request.PullRequest.RepositoryId,
                request.PullRequest.PullRequestId);

            _context.Submissions.Add(submission);
            await _context.CommitAsync(default);

            string message = $"""
            Submission created.
            {result.Submission.ToDisplayString()}
            """;

            await _notifier.SendCommentToPullRequest(message);
        }
        else
        {
            await _notifier.NotifySubmissionUpdate(result.Submission);
        }

        return new Response.Success();
    }

    private async Task<string> GetSubjectCourseAssignmentsString(
        PullRequestDto pullRequest,
        CancellationToken cancellationToken)
    {
        GithubSubjectCourse? subjectCourse = await _context.SubjectCourses
            .ForOrganization(pullRequest.OrganizationId, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (subjectCourse is null)
        {
            throw EntityNotFoundException.SubjectCourse().TaggedWithNotFound();
        }

        List<GithubAssignment> assignments = await _context.Assignments
            .QueryAsync(GithubAssignmentQuery.Build(x => x.WithSubjectCourseId(subjectCourse.Id)), cancellationToken)
            .ToListAsync(cancellationToken);

        IOrderedEnumerable<string> branchNames = assignments
            .Select(x => x.BranchName)
            .Order();

        return string.Join(", ", branchNames);
    }
}