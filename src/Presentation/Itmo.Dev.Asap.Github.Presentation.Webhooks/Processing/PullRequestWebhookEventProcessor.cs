using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Extensions;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents;
using Itmo.Dev.Asap.Github.Application.Models.PullRequests;
using MediatR;
using Microsoft.Extensions.Logging;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.PullRequest;

namespace Itmo.Dev.Asap.Github.Presentation.Webhooks.Processing;

public class PullRequestWebhookEventProcessor
{
    private readonly ILogger<PullRequestWebhookEventProcessor> _logger;
    private readonly IMediator _mediator;
    private readonly IPullRequestEventNotifier _notifier;

    public PullRequestWebhookEventProcessor(
        ILogger<PullRequestWebhookEventProcessor> logger,
        IMediator mediator,
        IPullRequestEventNotifier notifier)
    {
        _logger = logger;
        _mediator = mediator;
        _notifier = notifier;
    }

    public async Task ProcessAsync(PullRequestDto pullRequest, PullRequestEvent pullRequestEvent, string action)
    {
        ILogger logger = _logger.ToPullRequestLogger(pullRequest);

        const string processorName = nameof(PullRequestWebhookEventProcessor);

        logger.LogInformation(
            "{MethodName}: {EventName} with type {Action}",
            processorName,
            pullRequestEvent.GetType().Name,
            action);

        try
        {
            switch (action)
            {
                case PullRequestActionValue.Synchronize:
                case PullRequestActionValue.Opened:
                {
                    var command = new PullRequestUpdated.Command(pullRequest);
                    PullRequestUpdated.Response response = await _mediator.Send(command);

                    if (response is PullRequestUpdated.Response.StudentNotFound)
                    {
                        const string message = "Current repository is not attached to any student";
                        await _notifier.SendCommentToPullRequest(message);
                    }

                    if (response is PullRequestUpdated.Response.AssignmentNotFound assignmentNotFound)
                    {
                        (string branchName, string subjectCourseTitle, string assignments) = assignmentNotFound;

                        string message = $"""
                        Assignment with branch name '{branchName}' for subject course '{subjectCourseTitle}' was not found. 
                        Ensure that branch name is correct.
                        Available assignments: {assignments}
                        """;

                        await _notifier.SendCommentToPullRequest(message);
                    }

                    break;
                }

                case PullRequestActionValue.Reopened:
                {
                    var command = new PullRequestReopened.Command(pullRequest);
                    await _mediator.Send(command);

                    break;
                }

                case PullRequestActionValue.Closed:
                {
                    bool merged = pullRequestEvent.PullRequest.Merged ?? false;

                    var command = new PullRequestClosed.Command(pullRequest, merged);
                    await _mediator.Send(command);

                    break;
                }

                case PullRequestActionValue.Assigned:
                case PullRequestActionValue.ReviewRequestRemoved:
                case PullRequestActionValue.ReviewRequested:
                    logger.LogDebug("Skip pull request action with type {Action}", action);
                    break;

                default:
                    logger.LogWarning("Unsupported pull request webhook type was received: {Action}", action);
                    break;
            }
        }
        catch (Exception e)
        {
            string message = $"Failed to handle {action}";
            logger.LogError(e, "{MethodName}: {Message}", processorName, message);

            await _notifier.SendExceptionMessageSafe(e);
        }
    }
}