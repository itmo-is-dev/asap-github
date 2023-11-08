using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow.Results;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Common.Exceptions;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents.PullRequestChangesRequested;

namespace Itmo.Dev.Asap.Github.Application.PullRequestEvents;

internal class PullRequestChangesRequestedHandler : IRequestHandler<Command>
{
    private readonly ISubmissionWorkflowService _asapSubmissionWorkflowService;
    private readonly IPullRequestEventNotifier _notifier;
    private readonly IPersistenceContext _context;

    public PullRequestChangesRequestedHandler(
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
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(request.PullRequest.SenderId, cancellationToken);

        GithubSubmission submission = await _context.Submissions
            .GetSubmissionForPullRequestAsync(request.PullRequest, cancellationToken);

        SubmissionNotAcceptedResult result = await _asapSubmissionWorkflowService.SubmissionNotAcceptedAsync(
            issuer.Id,
            submission.Id,
            cancellationToken);

        string message = result switch
        {
            SubmissionNotAcceptedResult.Success => "Submission is not accepted by mentor",

            SubmissionNotAcceptedResult.InvalidState invalidState
                => new InvalidStateMessage("Changes request", invalidState.SubmissionState),

            _ => throw new UnexpectedOperationResultException(),
        };

        await _notifier.SendCommentToPullRequest(message);
    }
}