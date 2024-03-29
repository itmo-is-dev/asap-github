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

internal class PullRequestChangesRequestedHandler : IRequestHandler<Command, Response>
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
        return new Response.Success();
    }
}