using Octokit;

namespace Itmo.Dev.Asap.Github.Application.Octokit.Clients;

public interface IGithubClientProvider
{
    ValueTask<IGitHubClient> GetServiceClientAsync(CancellationToken cancellationToken);

    ValueTask<IGitHubClient> GetClientForInstallationAsync(long installationId, CancellationToken cancellationToken);

    ValueTask<IGitHubClient> GetOrganizationClientAsync(
        long organizationId,
        CancellationToken cancellationToken);
}