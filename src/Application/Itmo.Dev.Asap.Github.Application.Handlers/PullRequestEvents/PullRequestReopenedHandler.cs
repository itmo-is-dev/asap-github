using Itmo.Dev.Asap.Github.Application.Core.Services.SubmissionWorkflow;
using Itmo.Dev.Asap.Github.Application.Core.Services.SubmissionWorkflow.Results;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.Handlers.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Common.Exceptions;
using Itmo.Dev.Asap.Github.Domain.Submissions;
using Itmo.Dev.Asap.Github.Domain.Users;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents.PullRequestReopened;

namespace Itmo.Dev.Asap.Github.Application.Handlers.PullRequestEvents;

internal class PullRequestReopenedHandler : IRequestHandler<Command>
{
    private readonly ISubmissionWorkflowService _asapSubmissionWorkflowService;
    private readonly IPullRequestEventNotifier _notifier;
    private readonly IPersistenceContext _context;

    public PullRequestReopenedHandler(
        ISubmissionWorkflowService asapSubmissionWorkflowService,
        IPullRequestEventNotifier notifier,
        IPersistenceContext context)
    {
        _asapSubmissionWorkflowService = asapSubmissionWorkflowService;
        _notifier = notifier;
        _context = context;
    }

    public async Task Handle(Command request, CancellationToken cancellationToken)
    {
        GithubUser issuer = await _context.Users
            .GetForGithubIdAsync(request.PullRequest.SenderId, cancellationToken);

        GithubSubmission submission = await _context.Submissions
            .GetSubmissionForPullRequestAsync(request.PullRequest, cancellationToken);

        SubmissionReactivatedResult result = await _asapSubmissionWorkflowService.SubmissionReactivatedAsync(
            issuer.Id,
            submission.Id,
            cancellationToken);

        string message = result switch
        {
            SubmissionReactivatedResult.Success => "Submission activated successfully",

            SubmissionReactivatedResult.InvalidState invalidState
                => new InvalidStateMessage("Reopening pull request", invalidState.SubmissionState),

            _ => throw new UnexpectedOperationResultException(),
        };

        await _notifier.SendCommentToPullRequest(message);
    }
}