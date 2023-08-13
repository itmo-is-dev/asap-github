namespace Itmo.Dev.Asap.Github.Application.Octokit.Notifications;

public interface IPullRequestEventNotifier
{
    Task SendCommentToPullRequest(string message);
}