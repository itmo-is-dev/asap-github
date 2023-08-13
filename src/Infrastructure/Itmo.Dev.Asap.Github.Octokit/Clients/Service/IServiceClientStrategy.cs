namespace Itmo.Dev.Asap.Github.Octokit.Clients.Service;

public interface IServiceClientStrategy
{
    ValueTask<long> GetInstallationId();
}