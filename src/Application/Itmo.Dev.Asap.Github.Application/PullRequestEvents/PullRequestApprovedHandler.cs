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

internal class PullRequestApprovedHandler : IRequestHandler<Command, Response>
{
    private const string SubmissionCompletedMessage =
    """
    submission is alredy completed,
    pull request approve will be ignored 
    (if submission is rated, pull request can be safely merged)
    """;

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

    public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
    {
        GithubUser? issuer = await _context.Users
            .FindByGithubIdAsync(request.PullRequest.SenderId, cancellationToken);

        if (issuer is null)
            return new Response.IssuerNotFound();

        GithubSubmission? submission = await _context.Submissions
            .FindSubmissionForPullRequestAsync(request.PullRequest, cancellationToken);

        if (submission is null)
            return new Response.SubmissionNotFound();

        SubmissionApprovedResult result = await _submissionWorkflowService
            .SubmissionApprovedAsync(
                issuer.Id,
                submission.Id,
                cancellationToken);

        string message = result switch
        {
            SubmissionApprovedResult.Success success
                => $"Submission reviewed successfully\n\n{success.SubmissionRate.ToDisplayString()}",

            SubmissionApprovedResult.InvalidState { State: SubmissionState.Completed }
                => SubmissionCompletedMessage,

            SubmissionApprovedResult.InvalidState invalidState
                => new InvalidStateMessage("Pull request approved", invalidState.State),

            _ => throw new UnexpectedOperationResultException(),
        };

        await _notifier.SendCommentToPullRequest(message);
        return new Response.Success();
    }
}