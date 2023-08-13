namespace Itmo.Dev.Asap.Github.Presentation.Webhooks.Configuration;

public class GithubWebhooksConfiguration
{
    public bool Enabled { get; init; }

    public string Secret { get; init; } = string.Empty;
}