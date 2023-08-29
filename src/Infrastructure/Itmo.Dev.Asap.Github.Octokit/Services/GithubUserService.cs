using Itmo.Dev.Asap.Github.Application.Octokit.Clients;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Octokit;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

public class GithubUserService : IGithubUserService
{
    private readonly IGithubClientProvider _clientProvider;
    private readonly HttpClient _gitHubHttpClient;

    public GithubUserService(HttpClient gitHubHttpClient, IGithubClientProvider clientProvider)
    {
        _gitHubHttpClient = gitHubHttpClient;
        _clientProvider = clientProvider;
    }

    public async Task<bool> IsUserExistsAsync(string username, CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetClientAsync(cancellationToken);
        string token = client.Connection.Credentials.GetToken();

        using var message = new HttpRequestMessage(HttpMethod.Get, "/users?since=7121897&per_page=1");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _gitHubHttpClient.SendAsync(message, cancellationToken);

        JsonObject[]? users = await response.Content
            .ReadFromJsonAsync<JsonObject[]>(cancellationToken: cancellationToken);

        JsonObject? user = users?.SingleOrDefault();
        string? actualUsername = user?["login"]?.ToString();

        return actualUsername?.Equals(username, StringComparison.OrdinalIgnoreCase) is true;
    }
}