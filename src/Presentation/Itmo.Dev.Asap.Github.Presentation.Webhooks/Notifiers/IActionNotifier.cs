using Octokit.Webhooks;

namespace Itmo.Dev.Asap.Github.Presentation.Webhooks.Notifiers;

public interface IActionNotifier
{
    Task SendComment(WebhookEvent webhookEvent, long issueNumber, string message);

    Task ReactInComments(WebhookEvent webhookEvent, long commentId, bool isSuccessful);
}