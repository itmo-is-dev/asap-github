namespace Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;

public interface IPullRequestCommentEventNotifier : IPullRequestEventNotifier
{
    Task ReactToUserComment(bool isSuccess);
}