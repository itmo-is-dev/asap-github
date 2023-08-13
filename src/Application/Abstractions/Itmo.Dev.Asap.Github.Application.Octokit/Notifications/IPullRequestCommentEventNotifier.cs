namespace Itmo.Dev.Asap.Github.Application.Octokit.Notifications;

public interface IPullRequestCommentEventNotifier : IPullRequestEventNotifier
{
    Task ReactToUserComment(bool isSuccess);
}