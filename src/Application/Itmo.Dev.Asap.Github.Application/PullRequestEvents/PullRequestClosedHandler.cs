using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow.Results;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Common.Exceptions;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents.PullRequestClosed;

namespace Itmo.Dev.Asap.Github.Application.PullRequestEvents;

internal class PullRequestClosedHandler : IRequestHandler<Command, Response>
{
    private const string MergeOperation = "Merging pull request";
    private const string CloseOperation = "Closing pull request";

    private readonly ISubmissionWorkflowService _asapSubmissionWorkflowService;
    private readonly IPermissionService _asapPermissionService;
    private readonly IPullRequestEventNotifier _notifier;
    private readonly IPersistenceContext _context;

    public PullRequestClosedHandler(
        ISubmissionWorkflowService asapSubmissionWorkflowService,
        IPermissionService asapPermissionService,
        IPullRequestEventNotifier notifier,
        IPersistenceContext context)
    {
        _asapSubmissionWorkflowService = asapSubmissionWorkflowService;
        _asapPermissionService = asapPermissionService;
        _notifier = notifier;
        _context = context;
    }

    public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
    {
        GithubUser? issuer = await _context.Users.FindByGithubIdAsync(request.PullRequest.SenderId, cancellationToken);

        if (issuer is null)
            return new Response.IssuerNotFound();

        GithubSubmission? submission = await _context.Submissions
            .FindSubmissionForPullRequestAsync(request.PullRequest, cancellationToken);

        if (submission is null)
            return new Response.SubmissionNotFound();

        bool isOrganizationMentor = await _asapPermissionService.IsSubmissionMentorAsync(
            issuer.Id,
            submission.Id,
            cancellationToken);

#pragma warning disable IDE0072
        string message = (isOrganizationMentor, request.IsMerged) switch
        {
            (true, true) => await HandleAcceptedAsync(issuer, submission, cancellationToken),
            (true, false) => await HandleRejectedAsync(issuer, submission, cancellationToken),
            (false, var isMerged) => await HandleAbandonedAsync(issuer, submission, isMerged, cancellationToken),
        };
#pragma warning restore IDE0072

        await _notifier.SendCommentToPullRequest(message);
        return new Response.Success();
    }

    private async Task<string> HandleAcceptedAsync(
        GithubUser issuer,
        GithubSubmission submission,
        CancellationToken cancellationToken)
    {
        SubmissionAcceptedResult result = await _asapSubmissionWorkflowService.SubmissionAcceptedAsync(
            issuer.Id,
            submission.Id,
            cancellationToken);

        return result switch
        {
            SubmissionAcceptedResult.Success success
                => $"Submission accepted by mentor\n\n{success.SubmissionRate.ToDisplayString()}",

            SubmissionAcceptedResult.InvalidState invalidState
                => new InvalidStateMessage(MergeOperation, invalidState.SubmissionState),

            _ => throw new UnexpectedOperationResultException(),
        };
    }

    private async Task<string> HandleRejectedAsync(
        GithubUser issuer,
        GithubSubmission submission,
        CancellationToken cancellationToken)
    {
        SubmissionRejectedResult result = await _asapSubmissionWorkflowService.SubmissionRejectedAsync(
            issuer.Id,
            submission.Id,
            cancellationToken);

        return result switch
        {
            SubmissionRejectedResult.Success => "Submission rejected by mentor",

            SubmissionRejectedResult.InvalidState invalidState
                => new InvalidStateMessage(CloseOperation, invalidState.SubmissionState),

            _ => throw new UnexpectedOperationResultException(),
        };
    }

    private async Task<string> HandleAbandonedAsync(
        GithubUser issuer,
        GithubSubmission submission,
        bool isMerged,
        CancellationToken cancellationToken)
    {
        SubmissionAbandonedResult result = await _asapSubmissionWorkflowService.SubmissionAbandonedAsync(
            issuer.Id,
            submission.Id,
            isMerged,
            cancellationToken);

        return result switch
        {
            SubmissionAbandonedResult.Success => "Submission abandoned",

            SubmissionAbandonedResult.InvalidState invalidState
                => new InvalidStateMessage(isMerged ? MergeOperation : CloseOperation, invalidState.SubmissionState),

            _ => throw new UnexpectedOperationResultException(),
        };
    }
}