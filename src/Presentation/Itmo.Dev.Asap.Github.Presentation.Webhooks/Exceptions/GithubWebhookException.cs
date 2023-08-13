namespace Itmo.Dev.Asap.Github.Presentation.Webhooks.Exceptions;

public class GithubWebhookException : Exception
{
    public GithubWebhookException(string? message) : base(message) { }
}