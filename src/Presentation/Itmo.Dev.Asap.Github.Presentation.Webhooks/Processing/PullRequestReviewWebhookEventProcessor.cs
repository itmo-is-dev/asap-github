using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Extensions;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Parsers;
using Itmo.Dev.Asap.Github.Application.Models.PullRequests;
using MediatR;
using Microsoft.Extensions.Logging;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.PullRequestReview;
using Octokit.Webhooks.Models.PullRequestReviewEvent;

namespace Itmo.Dev.Asap.Github.Presentation.Webhooks.Processing;

public class PullRequestReviewWebhookEventProcessor
{
    private readonly ILogger<PullRequestReviewWebhookEventProcessor> _logger;
    private readonly IMediator _mediator;
    private readonly ISubmissionCommandParser _commandParser;
    private readonly IPullRequestEventNotifier _notifier;

    public PullRequestReviewWebhookEventProcessor(
        ILogger<PullRequestReviewWebhookEventProcessor> logger,
        IMediator mediator,
        ISubmissionCommandParser commandParser,
        IPullRequestEventNotifier notifier)
    {
        _logger = logger;
        _mediator = mediator;
        _commandParser = commandParser;
        _notifier = notifier;
    }

    public async Task ProcessAsync(
        PullRequestDto pullRequest,
        PullRequestReviewEvent reviewEvent,
        string action)
    {
        ILogger logger = _logger.ToPullRequestLogger(pullRequest);

        const string processorName = nameof(PullRequestReviewWebhookEventProcessor);

        logger.LogInformation(
            "{MethodName}: {Name} with type {Action}",
            processorName,
            reviewEvent.GetType().Name,
            action);

        try
        {
            string? reviewBody = reviewEvent.Review.Body;
            ReviewState? reviewState = reviewEvent.Review.State?.Value;

            switch (action)
            {
                case PullRequestReviewActionValue.Submitted when reviewState is ReviewState.Approved:
                    await ProcessApprovedAsync(pullRequest, reviewBody, reviewEvent.Review.Id);
                    break;

                case PullRequestReviewActionValue.Submitted when reviewState is ReviewState.ChangesRequested:
                    await ProcessRequestedChangesAsync(pullRequest, reviewBody, reviewEvent.Review.Id);
                    break;

                case PullRequestReviewActionValue.Submitted when reviewState is ReviewState.Commented:
                    await ProcessCommentedAsync(pullRequest, reviewBody, logger, _notifier, reviewEvent.Review.Id);
                    break;

                case PullRequestReviewActionValue.Edited:
                case PullRequestReviewActionValue.Dismissed:
                    logger.LogWarning("Pull request review action {Action} is not supported", action);
                    break;

                default:
                    logger.LogWarning("Pull request review for pr {PrLink} is not processed", pullRequest.Payload);
                    break;
            }
        }
        catch (Exception e)
        {
            string message = $"Failed to handle {action}";
            logger.LogError(e, "{MethodName}:{Message}", processorName, message);

            await _notifier.SendExceptionMessageSafe(e);
        }
    }

    private async Task ProcessApprovedAsync(PullRequestDto pullRequest, string? reviewBody, long commentId)
    {
        var command = new PullRequestApproved.Command(pullRequest);
        await _mediator.Send(command);

        if (reviewBody?.FirstOrDefault() is not '/')
            return;

        ISubmissionCommand submissionCommand = _commandParser.Parse(reviewBody);
        await ExecuteCommand(submissionCommand, pullRequest, commentId);
    }

    private async Task ProcessRequestedChangesAsync(PullRequestDto pullRequest, string? reviewBody, long commentId)
    {
        var command = new PullRequestChangesRequested.Command(pullRequest);
        await _mediator.Send(command);

        if (reviewBody?.FirstOrDefault() is not '/')
            return;

        ISubmissionCommand submissionCommand = _commandParser.Parse(reviewBody);
        await ExecuteCommand(submissionCommand, pullRequest, commentId);
    }

    private async Task ProcessCommentedAsync(
        PullRequestDto pullRequest,
        string? reviewBody,
        ILogger logger,
        IPullRequestEventNotifier notifier,
        long commentId)
    {
        if (reviewBody is null)
        {
            logger.LogInformation("Review body is null, skipping review comment");
            return;
        }

        if (reviewBody.FirstOrDefault() is not '/')
            return;

        ISubmissionCommand submissionCommand = _commandParser.Parse(reviewBody);
        await ExecuteCommand(submissionCommand, pullRequest, commentId);

        if (submissionCommand is not RateCommand)
        {
            const string message = "Review proceeded, but submission is not yet rated and still will be presented in queue.";
            await notifier.SendCommentToPullRequest(message);

            logger.LogInformation("Notify: {Message}", message);
        }
    }

    private async Task ExecuteCommand(
        ISubmissionCommand submissionCommand,
        PullRequestDto pullRequest,
        long commentId)
    {
        var command = new ExecuteSubmissionCommand.Command(pullRequest, commentId, submissionCommand);
        await _mediator.Send(command);
    }
}