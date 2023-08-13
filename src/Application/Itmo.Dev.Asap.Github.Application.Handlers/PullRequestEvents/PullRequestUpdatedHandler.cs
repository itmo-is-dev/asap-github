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

internal class PullRequestUpdatedHandler : IRequestHandler<Command>
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

    public async Task Handle(Command request, CancellationToken cancellationToken)
    {
        GithubUser issuer = await _context.Users.GetForUsernameAsync(request.PullRequest.Sender, cancellationToken);
        GithubUser user = await _context.Users.GetForUsernameAsync(request.PullRequest.Repository, cancellationToken);

        GithubAssignment? assignment = await _context.Assignments
            .FindAssignmentForPullRequestAsync(request.PullRequest, cancellationToken);

        if (assignment is null)
        {
            string message = await GetSubjectCourseAssignmentsString(request.PullRequest, cancellationToken);

            throw EntityNotFoundException.AssignmentWasNotFound(
                request.PullRequest.BranchName,
                request.PullRequest.Organization,
                message);
        }

        SubmissionUpdateResult result = await _submissionWorkflowService.SubmissionUpdatedAsync(
            issuer.Id,
            user.Id,
            assignment.Id,
            request.PullRequest.Payload,
            cancellationToken);

        if (result.IsCreated)
        {
            var submission = new GithubSubmission(
                result.Submission.Id,
                assignment.Id,
                user.Id,
                result.Submission.SubmissionDate,
                request.PullRequest.Organization,
                request.PullRequest.Repository,
                request.PullRequest.PullRequestNumber);

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
    }

    private async Task<string> GetSubjectCourseAssignmentsString(
        PullRequestDto pullRequest,
        CancellationToken cancellationToken)
    {
        GithubSubjectCourse? subjectCourse = await _context.SubjectCourses
            .ForOrganizationName(pullRequest.Organization, cancellationToken)
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