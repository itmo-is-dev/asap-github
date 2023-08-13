using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Commands;
using Itmo.Dev.Asap.Github.Application.Dto.PullRequests;
using Itmo.Dev.Asap.Github.Application.Octokit.Extensions;
using Itmo.Dev.Asap.Github.Application.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Commands.Parsers;
using Itmo.Dev.Asap.Github.Commands.SubmissionCommands;
using MediatR;
using Microsoft.Extensions.Logging;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.IssueComment;

namespace Itmo.Dev.Asap.Github.Presentation.Webhooks.Processing;

public class IssueCommentWebhookProcessor
{
    private readonly ILogger<IssueCommentWebhookProcessor> _logger;
    private readonly IMediator _mediator;
    private readonly ISubmissionCommandParser _commandParser;
    private readonly IPullRequestEventNotifier _notifier;

    public IssueCommentWebhookProcessor(
        ILogger<IssueCommentWebhookProcessor> logger,
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
        IssueCommentEvent issueCommentEvent,
        string action)
    {
        ILogger logger = _logger.ToPullRequestLogger(pullRequest);

        const string processorName = nameof(IssueCommentWebhookProcessor);

        logger.LogInformation(
            "{MethodName}: {EventName} with type {Action}",
            processorName,
            issueCommentEvent.GetType().Name,
            action);

        try
        {
            string issueCommentBody = issueCommentEvent.Comment.Body;

            switch (action)
            {
                case IssueCommentActionValue.Created:
                    await ProcessCreatedAsync(pullRequest, issueCommentBody, issueCommentEvent.Comment.Id);
                    break;

                case IssueCommentActionValue.Deleted:
                case IssueCommentActionValue.Edited:
                    logger.LogTrace("Pull request comment {Action} event will be ignored", action);
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

    private async Task ProcessCreatedAsync(PullRequestDto pullRequest, string issueCommentBody, long commentId)
    {
        if (issueCommentBody.FirstOrDefault() is not '/')
            return;

        ISubmissionCommand submissionCommand = _commandParser.Parse(issueCommentBody);

        var command = new ExecuteSubmissionCommand.Command(pullRequest, commentId, submissionCommand);
        await _mediator.Send(command);
    }
}