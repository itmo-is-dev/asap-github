using Octokit;

namespace Itmo.Dev.Asap.Github.Application.Octokit.Clients;

public interface IGithubClientProvider
{
    ValueTask<IGitHubClient> GetClientAsync(CancellationToken cancellationToken);

    ValueTask<IGitHubClient> GetClientForInstallationAsync(long installationId, CancellationToken cancellationToken);

    ValueTask<IGitHubClient> GetClientForOrganizationAsync(
        string organizationName,
        CancellationToken cancellationToken);
}