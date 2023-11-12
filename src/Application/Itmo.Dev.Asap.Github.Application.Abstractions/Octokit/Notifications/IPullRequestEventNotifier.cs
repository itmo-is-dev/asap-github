namespace Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;

public interface IPullRequestEventNotifier
{
    Task SendCommentToPullRequest(string message);
}