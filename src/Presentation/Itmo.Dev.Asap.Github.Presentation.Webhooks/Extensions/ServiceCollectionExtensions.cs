using Itmo.Dev.Asap.Github.Application.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Commands.Extensions;
using Itmo.Dev.Asap.Github.Presentation.Webhooks.Configuration;
using Itmo.Dev.Asap.Github.Presentation.Webhooks.Notifiers;
using Itmo.Dev.Asap.Github.Presentation.Webhooks.Processing;
using Microsoft.Extensions.DependencyInjection;
using Octokit.Webhooks;

namespace Itmo.Dev.Asap.Github.Presentation.Webhooks.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebhooksPresentation(this IServiceCollection collection)
    {
        collection.AddOptions<GithubWebhooksConfiguration>().BindConfiguration("Presentation:Webhooks");

        collection.AddScoped<EventNotifierProxy>();
        collection.AddScoped<IPullRequestEventNotifier>(x => x.GetRequiredService<EventNotifierProxy>());
        collection.AddScoped<IPullRequestCommentEventNotifier>(x => x.GetRequiredService<EventNotifierProxy>());

        collection.AddScoped<PullRequestWebhookEventProcessor>();
        collection.AddScoped<PullRequestReviewWebhookEventProcessor>();
        collection.AddScoped<IssueCommentWebhookProcessor>();

        collection.AddScoped<WebhookEventProcessor, AsapWebhookEventProcessor>();

        collection.AddScoped<IActionNotifier, ActionNotifier>();
        collection.AddPresentationCommands();

        return collection;
    }
}