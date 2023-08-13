using Itmo.Dev.Asap.Github.Application.Core.Models;
using Itmo.Dev.Asap.Github.Application.Core.Services;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Domain.Submissions;
using Itmo.Dev.Asap.Github.Domain.Users;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents.PullRequestClosed;

namespace Itmo.Dev.Asap.Github.Application.Handlers.PullRequestEvents;

#pragma warning disable IDE0072

internal class PullRequestClosedHandler : IRequestHandler<Command>
{
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

    public async Task Handle(Command request, CancellationToken cancellationToken)
    {
        GithubUser issuer = await _context.Users.GetForUsernameAsync(request.PullRequest.Sender, cancellationToken);

        GithubSubmission submission = await _context.Submissions
            .GetSubmissionForPullRequestAsync(request.PullRequest, cancellationToken);

        bool isOrganizationMentor = await _asapPermissionService.IsSubmissionMentorAsync(
            issuer.Id,
            submission.Id,
            cancellationToken);

        SubmissionActionMessageDto result = (isOrganizationMentor, request.IsMerged) switch
        {
            (true, true) => await _asapSubmissionWorkflowService.SubmissionAcceptedAsync(
                issuer.Id,
                submission.Id,
                cancellationToken),

            (true, false) => await _asapSubmissionWorkflowService.SubmissionRejectedAsync(
                issuer.Id,
                submission.Id,
                cancellationToken),

            (false, var isMerged) => await _asapSubmissionWorkflowService.SubmissionAbandonedAsync(
                issuer.Id,
                submission.Id,
                isMerged,
                cancellationToken),
        };

        await _notifier.SendCommentToPullRequest(result.Message);
    }
}