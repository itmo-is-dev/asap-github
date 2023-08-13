using Itmo.Dev.Asap.Github.Application.Core.Models;
using Itmo.Dev.Asap.Github.Application.Core.Services;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Domain.Submissions;
using Itmo.Dev.Asap.Github.Domain.Users;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents.PullRequestApproved;

namespace Itmo.Dev.Asap.Github.Application.Handlers.PullRequestEvents;

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
        GithubUser issuer = await _context.Users.GetForUsernameAsync(request.PullRequest.Sender, cancellationToken);

        GithubSubmission submission = await _context.Submissions
            .GetSubmissionForPullRequestAsync(request.PullRequest, cancellationToken);

        SubmissionActionMessageDto result = await _submissionWorkflowService.SubmissionApprovedAsync(
            issuer.Id,
            submission.Id,
            cancellationToken);

        await _notifier.SendCommentToPullRequest(result.Message);
    }
}