using Itmo.Dev.Asap.Github.Application.Octokit.Clients;
using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Octokit.Clients;
using Microsoft.Extensions.Logging;
using Octokit;
using System.Net;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

internal class GithubUserService : IGithubUserService
{
    private readonly IGithubClientProvider _clientProvider;
    private readonly GithubApiClient _apiClient;
    private readonly ILogger<GithubUserService> _logger;

    public GithubUserService(
        IGithubClientProvider clientProvider,
        GithubApiClient apiClient,
        ILogger<GithubUserService> logger)
    {
        _clientProvider = clientProvider;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<GithubUserModel?> FindByIdAsync(long userId, CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetServiceClientAsync(cancellationToken);
        string token = client.Connection.Credentials.GetToken();

        return await _apiClient.FindUserByIdAsync(userId, token, cancellationToken);
    }

    public async Task<GithubUserModel?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetServiceClientAsync(cancellationToken);

        try
        {
            User user = await client.User.Get(username);
            return new GithubUserModel(user.Id, user.Login);
        }
        catch (ApiException e) when (e.StatusCode is HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to find github user = {Username}", username);
            return null;
        }
    }
}