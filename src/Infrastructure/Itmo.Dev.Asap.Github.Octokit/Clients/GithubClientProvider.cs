using Itmo.Dev.Asap.Github.Application.Octokit.Clients;
using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Octokit.Clients.Service;
using Itmo.Dev.Asap.Github.Octokit.CredentialStores;
using Octokit;
using System.Collections.Concurrent;

namespace Itmo.Dev.Asap.Github.Octokit.Clients;

internal class GithubClientProvider : IGithubClientProvider
{
    private readonly IGitHubClient _client;
    private readonly Lazy<Task<long>> _serviceInstallationId;

    private readonly ConcurrentDictionary<long, IGitHubClient> _installationCache;
    private readonly ConcurrentDictionary<long, IGitHubClient> _organizationCache;

    private readonly GithubApiClient _apiClient;

    public GithubClientProvider(
        IGitHubClient client,
        IServiceClientStrategy serviceClientStrategy,
        GithubApiClient apiClient)
    {
        _client = client;
        _apiClient = apiClient;

        _installationCache = new ConcurrentDictionary<long, IGitHubClient>();
        _organizationCache = new ConcurrentDictionary<long, IGitHubClient>();

        _serviceInstallationId = new Lazy<Task<long>>(async () => await serviceClientStrategy.GetInstallationId());
    }

    public async ValueTask<IGitHubClient> GetServiceClientAsync(CancellationToken cancellationToken)
    {
        long installationId = await _serviceInstallationId.Value;
        return await GetClientForInstallationAsync(installationId, cancellationToken);
    }

    public ValueTask<IGitHubClient> GetClientForInstallationAsync(
        long installationId,
        CancellationToken cancellationToken)
    {
        IGitHubClient client = _installationCache.GetOrAdd(installationId, CreateClient);
        return ValueTask.FromResult(client);
    }

    public async ValueTask<IGitHubClient> GetOrganizationClientAsync(
        long organizationId,
        CancellationToken cancellationToken)
    {
        if (_organizationCache.TryGetValue(organizationId, out IGitHubClient? client))
            return client;

        IGitHubClient serviceClient = await GetServiceClientAsync(cancellationToken);

        GithubOrganizationModel? organization = await _apiClient.FindOrganizationByIdAsync(
            organizationId,
            serviceClient.Connection.Credentials.GetToken(),
            cancellationToken);

        if (organization is null)
            throw new InvalidOperationException("Organization does not exist or has no installation");

        Installation installation = await _client.GitHubApps.GetOrganizationInstallationForCurrent(organization.Name);
        client = await GetClientForInstallationAsync(installation.Id, cancellationToken);

        _organizationCache[organizationId] = client;

        return client;
    }

    private IGitHubClient CreateClient(long installationId)
    {
        return new GitHubClient(
            new ProductHeaderValue($"Installation-{installationId}"),
            new InstallationCredentialStore(_client, installationId));
    }
}