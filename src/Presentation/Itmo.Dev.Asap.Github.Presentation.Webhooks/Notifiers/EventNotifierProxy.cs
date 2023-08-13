using Itmo.Dev.Asap.Github.Application.Dto.PullRequests;
using Itmo.Dev.Asap.Github.Application.Octokit.Extensions;
using Itmo.Dev.Asap.Github.Application.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Presentation.Webhooks.Exceptions;
using Microsoft.Extensions.Logging;
using Octokit.Webhooks.Events;

namespace Itmo.Dev.Asap.Github.Presentation.Webhooks.Notifiers;

public class EventNotifierProxy : IPullRequestCommentEventNotifier
{
    private readonly IActionNotifier _actionNotifier;
    private readonly ILogger<EventNotifierProxy> _logger;
    private IPullRequestEventNotifier? _eventNotifier;
    private IPullRequestCommentEventNotifier? _commentEventNotifier;

    public EventNotifierProxy(IActionNotifier actionNotifier, ILogger<EventNotifierProxy> logger)
    {
        _actionNotifier = actionNotifier;
        _logger = logger;
    }

    public Task SendCommentToPullRequest(string message)
    {
        return _eventNotifier?.SendCommentToPullRequest(message)
               ?? _commentEventNotifier?.SendCommentToPullRequest(message)
               ?? throw new GithubWebhookException("Notifier uninitialized");
    }

    public Task ReactToUserComment(bool isSuccess)
    {
        return _commentEventNotifier?.ReactToUserComment(isSuccess)
               ?? throw new GithubWebhookException("Notifier uninitialized");
    }

    public void FromPullRequestEvent(PullRequestEvent pullRequestEvent, PullRequestDto pullRequest)
    {
        ILogger logger = _logger.ToPullRequestLogger(pullRequest);

        _eventNotifier = new PullRequestEventNotifier(
            _actionNotifier,
            pullRequestEvent,
            pullRequestEvent.PullRequest.Number,
            logger);

        _commentEventNotifier = null;
    }

    public void FromPullRequestReviewEvent(PullRequestReviewEvent reviewEvent, PullRequestDto pullRequest)
    {
        ILogger logger = _logger.ToPullRequestLogger(pullRequest);

        _eventNotifier = new PullRequestEventNotifier(
            _actionNotifier,
            reviewEvent,
            reviewEvent.PullRequest.Number,
            logger);

        _commentEventNotifier = null;
    }

    public void FromIssueCommentEvent(IssueCommentEvent commentEvent, PullRequestDto pullRequest)
    {
        ILogger logger = _logger.ToPullRequestLogger(pullRequest);

        _commentEventNotifier = new PullRequestCommentEventNotifier(
            _actionNotifier,
            commentEvent,
            commentEvent.Comment.Id,
            commentEvent.Issue.Number,
            logger);

        _eventNotifier = null;
    }
}