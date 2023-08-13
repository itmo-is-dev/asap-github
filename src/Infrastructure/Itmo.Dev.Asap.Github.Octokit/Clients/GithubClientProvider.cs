using Itmo.Dev.Asap.Github.Application.Octokit.Clients;
using Itmo.Dev.Asap.Github.Common.Extensions;
using Itmo.Dev.Asap.Github.Common.Tools;
using Itmo.Dev.Asap.Github.Octokit.Clients.Service;
using Itmo.Dev.Asap.Github.Octokit.CredentialStores;
using Octokit;

namespace Itmo.Dev.Asap.Github.Octokit.Clients;

public class GithubClientProvider : IGithubClientProvider
{
    private readonly IGitHubClient _client;
    private readonly IGithubMemoryCache _cache;
    private readonly Lazy<Task<long>> _serviceInstallationId;

    public GithubClientProvider(
        IGitHubClient client,
        IGithubMemoryCache cache,
        IServiceClientStrategy serviceClientStrategy)
    {
        _client = client;
        _cache = cache;

        _serviceInstallationId = new Lazy<Task<long>>(async () => await serviceClientStrategy.GetInstallationId());
    }

    public async ValueTask<IGitHubClient> GetClientAsync(CancellationToken cancellationToken)
    {
        long installationId = await _serviceInstallationId.Value;
        return await GetClientForInstallationAsync(installationId, cancellationToken);
    }

    public ValueTask<IGitHubClient> GetClientForInstallationAsync(
        long installationId,
        CancellationToken cancellationToken)
    {
        IGitHubClient client = _cache.GetOrCreateNotNull(installationId, _ => CreateClient(installationId));
        return ValueTask.FromResult(client);
    }

    public async ValueTask<IGitHubClient> GetClientForOrganizationAsync(
        string organizationName,
        CancellationToken cancellationToken)
    {
        Installation installation = await _client.GitHubApps.GetOrganizationInstallationForCurrent(organizationName);
        return await GetClientForInstallationAsync(installation.Id, cancellationToken);
    }

    private IGitHubClient CreateClient(long installationId)
    {
        return new GitHubClient(
            new ProductHeaderValue($"Installation-{installationId}"),
            new InstallationCredentialStore(_client, installationId));
    }
}