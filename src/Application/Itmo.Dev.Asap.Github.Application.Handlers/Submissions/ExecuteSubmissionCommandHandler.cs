using Itmo.Dev.Asap.Github.Application.CommandExecution;
using Itmo.Dev.Asap.Github.Application.Core.Services.Submissions;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Commands.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Commands.ExecuteSubmissionCommand;

namespace Itmo.Dev.Asap.Github.Application.Handlers.Submissions;

internal class ExecuteSubmissionCommandHandler : IRequestHandler<Command>
{
    private readonly ISubmissionService _asapSubmissionService;
    private readonly ILogger<ExecuteSubmissionCommandHandler> _logger;
    private readonly IPullRequestCommentEventNotifier _notifier;
    private readonly IPersistenceContext _context;

    public ExecuteSubmissionCommandHandler(
        ISubmissionService asapSubmissionService,
        ILogger<ExecuteSubmissionCommandHandler> logger,
        IPullRequestCommentEventNotifier notifier,
        IPersistenceContext context)
    {
        _asapSubmissionService = asapSubmissionService;
        _logger = logger;
        _notifier = notifier;
        _context = context;
    }

    public async Task Handle(Command request, CancellationToken cancellationToken)
    {
        var visitor = new PullRequestContextCommandVisitor(
            _asapSubmissionService,
            request.PullRequest,
            _notifier,
            _context);

        try
        {
            await ProcessCommandAsync(request, visitor);
        }
        catch (Exception e)
        {
            const string message = "An internal error occurred while processing command. Contact support for details.";

            _logger.LogError(e, "{Message}", message);

            await _notifier.SendCommentToPullRequest(message);
            await _notifier.ReactToUserComment(false);
        }
    }

    private async Task ProcessCommandAsync(Command request, PullRequestContextCommandVisitor visitor)
    {
        SubmissionCommandResult result = await request.SubmissionCommand.AcceptAsync(visitor);

        if (result is SubmissionCommandResult.Success)
        {
            await _notifier.ReactToUserComment(true);
            return;
        }

        if (result is SubmissionCommandResult.Failure failure)
        {
            string commandName = request.SubmissionCommand.Name;
            string title = $"Error occured while processing {commandName} command";
            string message = $"{title} {failure.Message}";

            _logger.LogWarning("{Title}: {Message}", title, message);

            await _notifier.SendCommentToPullRequest(message);
            await _notifier.ReactToUserComment(false);

            return;
        }

        _logger.LogError(
            "Unknown submission command result. Type = {Type}, Value = {Value}",
            result.GetType(),
            JsonConvert.SerializeObject(result));
    }
}