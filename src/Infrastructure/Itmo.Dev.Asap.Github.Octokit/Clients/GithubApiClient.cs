using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Octokit.Extensions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace Itmo.Dev.Asap.Github.Octokit.Clients;

internal class GithubApiClient
{
    private readonly HttpClient _githubHttpClient;
    private readonly JsonSerializer _serializer;

    public GithubApiClient(HttpClient githubHttpClient, JsonSerializer serializer)
    {
        _githubHttpClient = githubHttpClient;
        _serializer = serializer;
    }

    public async Task<GithubOrganizationModel?> FindOrganizationByIdAsync(
        long organizationId,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/organizations/{organizationId}");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        HttpResponseMessage response = await _githubHttpClient.SendAsync(message, cancellationToken);

        if (response.StatusCode is HttpStatusCode.NotFound)
            return null;

        await using Stream content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(content);
        await using var jsonReader = new JsonTextReader(reader);

        return _serializer.Deserialize<GithubOrganizationModel>(jsonReader);
    }

    public async Task<GithubTeamModel?> FindTeamByIdAsync(
        long organizationId,
        long teamId,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Get,
            $"/organizations/{organizationId}/team/{teamId}");

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        HttpResponseMessage response = await _githubHttpClient.SendAsync(message, cancellationToken);

        if (response.StatusCode is HttpStatusCode.NotFound)
            return null;

        await using Stream content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(content);
        await using var jsonReader = new JsonTextReader(reader);

        return _serializer.Deserialize<GithubTeamModel>(jsonReader);
    }

    public async Task<GithubUserModel?> FindUserByIdAsync(
        long userId,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "/users?since=7121897&per_page=1");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        HttpResponseMessage response = await _githubHttpClient.SendAsync(message, cancellationToken);

        if (response.StatusCode is HttpStatusCode.NotFound)
            return null;

        await using Stream content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(content);
        await using var jsonReader = new JsonTextReader(reader);

        GithubUserModel[]? users = _serializer.Deserialize<GithubUserModel[]>(jsonReader);

        return users is [GithubUserModel user] && user.Id.Equals(userId) ? user : null;
    }

    public async Task<GithubRepositoryModel?> FindRepositoryByIdAsync(
        long repositoryId,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/repositories/{repositoryId}");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        HttpResponseMessage response = await _githubHttpClient.SendAsync(message, cancellationToken);

        if (response.StatusCode is HttpStatusCode.NotFound)
            return null;

        await using Stream content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(content);
        await using var jsonReader = new JsonTextReader(reader);

        return _serializer.Deserialize<GithubRepositoryModel>(jsonReader);
    }

    /// <summary>
    ///     https://docs.github.com/en/rest/teams/teams?apiVersion=2022-11-28#add-or-update-team-repository-permissions
    /// </summary>
    public async Task UpdateTeamPermissionsAsync(
        long organizationId,
        long teamId,
        string repositoryOwner,
        string repositoryName,
        RepositoryPermission permission,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Put,
            $"/organizations/{organizationId}/team/{teamId}/repos/{repositoryOwner}/{repositoryName}");

        string content = $$"""
        {'permission':"{{permission.ToGithubApiString()}}"}
        """;

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        message.Content = new StringContent(content);

        await _githubHttpClient.SendAsync(message, cancellationToken);
    }
}