using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Clients;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Extensions;
using Itmo.Dev.Asap.Github.Application.Models.PullRequests;
using Itmo.Dev.Asap.Github.Presentation.Webhooks.Notifiers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.IssueComment;
using Octokit.Webhooks.Events.PullRequest;
using Octokit.Webhooks.Events.PullRequestReview;
using Octokit.Webhooks.Models;
using PullRequestReviewEvent = Octokit.Webhooks.Events.PullRequestReviewEvent;

namespace Itmo.Dev.Asap.Github.Presentation.Webhooks.Processing;

public class AsapWebhookEventProcessor : WebhookEventProcessor
{
    private readonly IGithubClientProvider _clientProvider;
    private readonly ILogger<AsapWebhookEventProcessor> _logger;
    private readonly EventNotifierProxy _notifierProxy;

    private readonly PullRequestWebhookEventProcessor _pullRequestWebhookEventProcessor;
    private readonly PullRequestReviewWebhookEventProcessor _pullRequestReviewWebhookEventProcessor;
    private readonly IssueCommentWebhookProcessor _issueCommentWebhookProcessor;

    public AsapWebhookEventProcessor(
        ILogger<AsapWebhookEventProcessor> logger,
        IGithubClientProvider clientProvider,
        PullRequestWebhookEventProcessor pullRequestWebhookEventProcessor,
        PullRequestReviewWebhookEventProcessor pullRequestReviewWebhookEventProcessor,
        IssueCommentWebhookProcessor issueCommentWebhookProcessor,
        EventNotifierProxy notifierProxy)
    {
        _logger = logger;
        _clientProvider = clientProvider;
        _pullRequestWebhookEventProcessor = pullRequestWebhookEventProcessor;
        _pullRequestReviewWebhookEventProcessor = pullRequestReviewWebhookEventProcessor;
        _issueCommentWebhookProcessor = issueCommentWebhookProcessor;
        _notifierProxy = notifierProxy;
    }

    protected override async Task ProcessPullRequestWebhookAsync(
        WebhookHeaders headers,
        PullRequestEvent pullRequestEvent,
        PullRequestAction action)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            string serialized = JsonConvert.SerializeObject(pullRequestEvent);
            _logger.LogTrace("Received github webhook pull request event, payload = {Payload}", serialized);
        }

        PullRequestDto pullRequest = CreateDescriptor(pullRequestEvent);
        ILogger repositoryLogger = _logger.ToPullRequestLogger(pullRequest);

        _notifierProxy.FromPullRequestEvent(pullRequestEvent, pullRequest);

        const string methodName = nameof(ProcessPullRequestWebhookAsync);

        if (IsSenderBotOrNull(pullRequestEvent))
        {
            repositoryLogger.LogTrace($"{methodName} was skipped because sender is bot or null");
            return;
        }

        await _pullRequestWebhookEventProcessor.ProcessAsync(
            pullRequest,
            pullRequestEvent,
            action);
    }

    protected override async Task ProcessPullRequestReviewWebhookAsync(
        WebhookHeaders headers,
        PullRequestReviewEvent pullRequestReviewEvent,
        PullRequestReviewAction action)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            string serialized = JsonConvert.SerializeObject(pullRequestReviewEvent);
            _logger.LogTrace("Received github webhook review event, payload = {Payload}", serialized);
        }

        PullRequestDto pullRequest = CreateDescriptor(pullRequestReviewEvent);
        ILogger repositoryLogger = _logger.ToPullRequestLogger(pullRequest);

        _notifierProxy.FromPullRequestReviewEvent(pullRequestReviewEvent, pullRequest);

        const string methodName = nameof(ProcessPullRequestReviewWebhookAsync);

        if (IsSenderBotOrNull(pullRequestReviewEvent))
        {
            repositoryLogger.LogTrace($"{methodName} was skipped because sender is bot or null");
            return;
        }

        await _pullRequestReviewWebhookEventProcessor.ProcessAsync(
            pullRequest,
            pullRequestReviewEvent,
            action);
    }

    protected override async Task ProcessIssueCommentWebhookAsync(
        WebhookHeaders headers,
        IssueCommentEvent issueCommentEvent,
        IssueCommentAction action)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            string serialized = JsonConvert.SerializeObject(issueCommentEvent);
            _logger.LogTrace("Received github webhook issue comment event, payload = {Payload}", serialized);
        }

        PullRequestDto pullRequest = await GetPullRequestDescriptor(issueCommentEvent);
        ILogger repositoryLogger = _logger.ToPullRequestLogger(pullRequest);

        _notifierProxy.FromIssueCommentEvent(issueCommentEvent, pullRequest);

        const string methodName = nameof(ProcessIssueCommentWebhookAsync);

        if (IsSenderBotOrNull(issueCommentEvent))
        {
            repositoryLogger.LogTrace($"{methodName} was skipped because sender is bot or null");
            return;
        }

        if (IsPullRequestCommand(issueCommentEvent) is false)
        {
            repositoryLogger.LogTrace(
                "Skipping commit in {IssueId}. Issue comments is not supported",
                issueCommentEvent.Issue.Id);

            return;
        }

        await _issueCommentWebhookProcessor.ProcessAsync(pullRequest, issueCommentEvent, action);
    }

    private static bool IsPullRequestCommand(IssueCommentEvent issueCommentEvent)
    {
        return issueCommentEvent.Issue.PullRequest.Url is not null;
    }

    private static bool IsSenderBotOrNull(WebhookEvent webhookEvent)
    {
        return webhookEvent.Sender is null || webhookEvent.Sender.Type == UserType.Bot;
    }

    private static PullRequestDto CreateDescriptor(PullRequestReviewEvent evt)
    {
        return new PullRequestDto(
            SenderId: evt.Sender!.Id,
            SenderUsername: evt.Sender!.Login,
            Payload: evt.Review.HtmlUrl,
            OrganizationId: evt.Organization!.Id,
            OrganizationName: evt.Organization!.Login,
            RepositoryId: evt.Repository!.Id,
            RepositoryName: evt.Repository!.Name,
            BranchName: evt.PullRequest.Head.Ref,
            PullRequestId: evt.PullRequest.Number,
            CommitHash: evt.PullRequest.Head.Sha);
    }

    private PullRequestDto CreateDescriptor(PullRequestEvent evt)
    {
        return new PullRequestDto(
            SenderId: evt.Sender!.Id,
            SenderUsername: evt.Sender!.Login,
            Payload: evt.PullRequest.HtmlUrl,
            OrganizationId: evt.Organization!.Id,
            OrganizationName: evt.Organization!.Login,
            RepositoryId: evt.Repository!.Id,
            RepositoryName: evt.Repository!.Name,
            BranchName: evt.PullRequest.Head.Ref,
            PullRequestId: evt.PullRequest.Number,
            CommitHash: evt.PullRequest.Head.Sha);
    }

    private async Task<PullRequestDto> GetPullRequestDescriptor(IssueCommentEvent evt)
    {
        IGitHubClient gitHubClient = await _clientProvider
            .GetOrganizationClientAsync(evt.Organization!.Id, default);

        PullRequest pullRequest = await gitHubClient.PullRequest
            .Get(evt.Repository!.Id, (int)evt.Issue.Number);

        return new PullRequestDto(
            SenderId: evt.Sender!.Id,
            SenderUsername: evt.Sender!.Login,
            Payload: pullRequest.HtmlUrl,
            OrganizationId: evt.Organization!.Id,
            OrganizationName: evt.Organization!.Login,
            RepositoryId: evt.Repository!.Id,
            RepositoryName: evt.Repository!.Name,
            BranchName: pullRequest.Head.Ref,
            PullRequestId: pullRequest.Number,
            CommitHash: pullRequest.Head.Sha);
    }
}