using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow.Results;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Extensions;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Models.Assignments;
using Itmo.Dev.Asap.Github.Application.Models.PullRequests;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using static Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents.PullRequestUpdated;

namespace Itmo.Dev.Asap.Github.Application.PullRequestEvents;

internal class PullRequestUpdatedHandler : IRequestHandler<Command, Response>
{
    private readonly ILogger<PullRequestUpdatedHandler> _logger;
    private readonly ISubmissionWorkflowService _submissionWorkflowService;
    private readonly IPullRequestEventNotifier _notifier;
    private readonly IPersistenceContext _context;

    public PullRequestUpdatedHandler(
        ILogger<PullRequestUpdatedHandler> logger,
        ISubmissionWorkflowService submissionWorkflowService,
        IPullRequestEventNotifier notifier,
        IPersistenceContext context)
    {
        _submissionWorkflowService = submissionWorkflowService;
        _notifier = notifier;
        _context = context;
        _logger = logger;
    }

    public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
    {
        GithubUser? issuer = await _context.Users
            .FindByGithubIdAsync(request.PullRequest.SenderId, cancellationToken);

        if (issuer is null)
            return new Response.IssuerNotFound();

        GithubSubjectCourseStudent? student = await _context.SubjectCourses
            .FindSubjectCourseStudentByRepositoryId(request.PullRequest.RepositoryId, cancellationToken);

        if (student is null)
            return new Response.StudentNotFound();

        GithubAssignment? assignment = await _context.Assignments
            .FindAssignmentForPullRequestAsync(request.PullRequest, cancellationToken);

        if (assignment is null)
        {
            string message = await GetSubjectCourseAssignmentsString(request.PullRequest, cancellationToken);

            return new Response.AssignmentNotFound(
                request.PullRequest.BranchName,
                request.PullRequest.OrganizationName,
                message);
        }

        SubmissionUpdatedResult result = await _submissionWorkflowService.SubmissionUpdatedAsync(
            issuer.Id,
            student.User.Id,
            assignment.Id,
            request.PullRequest.Payload,
            cancellationToken);

        if (result is not SubmissionUpdatedResult.Success success)
            throw new UnexpectedOperationResultException();

        if (success.IsCreated)
        {
            var submission = new GithubSubmission(
                success.SubmissionRate.Id,
                assignment.Id,
                student.User.Id,
                success.SubmissionRate.SubmissionDate,
                request.PullRequest.OrganizationId,
                request.PullRequest.RepositoryId,
                request.PullRequest.PullRequestId,
                request.PullRequest.CommitHash);

            _context.Submissions.Add(submission);
            await _context.CommitAsync(default);

            string message = $"""
                              Submission created.
                              {success.SubmissionRate.ToDisplayString()}
                              """;

            await _notifier.SendCommentToPullRequest(message);
        }
        else
        {
            _context.Submissions.UpdateCommitHash(success.SubmissionRate.Id, request.PullRequest.CommitHash);
            await _context.CommitAsync(default);

            await _notifier.NotifySubmissionUpdate(success.SubmissionRate);
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
            _logger.LogWarning("Subject course is not found");
            return string.Empty;
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