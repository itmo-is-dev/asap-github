using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public interface IErrorMessage
{
    Task WriteMessage(IPullRequestCommentEventNotifier notifier);
    string? ToString();
}