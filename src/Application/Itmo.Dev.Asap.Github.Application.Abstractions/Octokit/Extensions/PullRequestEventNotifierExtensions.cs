using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Common.Exceptions;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Extensions;

public static class PullRequestEventNotifierExtensions
{
    public static async Task NotifySubmissionUpdate(
        this IPullRequestEventNotifier pullRequestCommitEventNotifier,
        SubmissionRateDto submission)
    {
        string message = $"Submission {submission.Code} was updated." +
                         $"\nState: {submission.State}" +
                         $"\nDate: {submission.SubmissionDate}";

        await pullRequestCommitEventNotifier.SendCommentToPullRequest(message);
    }

    public static async Task SendExceptionMessageSafe(
        this IPullRequestEventNotifier notifier,
        Exception exception)
    {
        string message = exception switch
        {
            AsapGithubException e => e.Message,
            _ => "An internal error occurred while processing command. Contact support for details.",
        };

        await notifier.SendCommentToPullRequest(message);
    }
}