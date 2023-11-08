using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow.Results;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Common.Exceptions;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents.PullRequestApproved;

namespace Itmo.Dev.Asap.Github.Application.PullRequestEvents;

internal class PullRequestApprovedHandler : IRequestHandler<Command>
{
    private readonly ISubmissionWorkflowService _submissionWorkflowService;
    private readonly IPullRequestEventNotifier _notifier;
    private readonly IPersistenceContext _context;

    public PullRequestApprovedHandler(
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
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(request.PullRequest.SenderId, cancellationToken);

        GithubSubmission submission = await _context.Submissions
            .GetSubmissionForPullRequestAsync(request.PullRequest, cancellationToken);

        SubmissionApprovedResult result = await _submissionWorkflowService.SubmissionApprovedAsync(
            issuer.Id,
            submission.Id,
            cancellationToken);

        string message = result switch
        {
            SubmissionApprovedResult.Success success
                => $"Submission reviewed successfully\n\n{success.SubmissionRate.ToDisplayString()}",

            SubmissionApprovedResult.InvalidState invalidState
                => new InvalidStateMessage("Pull request approve", invalidState.State),

            _ => throw new UnexpectedOperationResultException(),
        };

        await _notifier.SendCommentToPullRequest(message);
    }
}