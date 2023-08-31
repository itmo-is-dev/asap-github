using Itmo.Dev.Asap.Github.Application.Octokit.Clients;
using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Octokit.Clients;
using Octokit;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

internal class GithubUserService : IGithubUserService
{
    private readonly IGithubClientProvider _clientProvider;
    private readonly GithubApiClient _apiClient;

    public GithubUserService(
        IGithubClientProvider clientProvider,
        GithubApiClient apiClient)
    {
        _clientProvider = clientProvider;
        _apiClient = apiClient;
    }

    public async Task<GithubUserModel?> FindByIdAsync(long userId, CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetClientAsync(cancellationToken);
        string token = client.Connection.Credentials.GetToken();

        return await _apiClient.FindUserByIdAsync(userId, token, cancellationToken);
    }
}