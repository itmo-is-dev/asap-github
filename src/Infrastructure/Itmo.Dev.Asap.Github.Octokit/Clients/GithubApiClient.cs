using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Octokit.Extensions;
using Itmo.Dev.Asap.Github.Octokit.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace Itmo.Dev.Asap.Github.Octokit.Clients;

internal class GithubApiClient
{
    private readonly HttpClient _githubHttpClient;
    private readonly JsonSerializer _serializer;
    private readonly ILogger<GithubApiClient> _logger;

    public GithubApiClient(HttpClient githubHttpClient, JsonSerializer serializer, ILogger<GithubApiClient> logger)
    {
        _githubHttpClient = githubHttpClient;
        _serializer = serializer;
        _logger = logger;
    }

    public async Task<GithubOrganizationModel?> FindOrganizationByIdAsync(
        long organizationId,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/organizations/{organizationId}");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        HttpResponseMessage response = await _githubHttpClient.SendAsync(message, cancellationToken);

        if (IsValidResponse(response) is false)
            return null;

        return await response.Content.DeserializeAsync<GithubOrganizationModel>(_serializer, cancellationToken);
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

        if (IsValidResponse(response) is false)
            return null;

        return await response.Content.DeserializeAsync<GithubTeamModel>(_serializer, cancellationToken);
    }

    public async Task<GithubUserModel?> FindUserByIdAsync(
        long userId,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/users?since={userId - 1}&per_page=1");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        HttpResponseMessage response = await _githubHttpClient.SendAsync(message, cancellationToken);

        if (IsValidResponse(response) is false)
            return null;

        GithubUserSerializationModel[]? users = await response.Content
            .DeserializeAsync<GithubUserSerializationModel[]>(_serializer, cancellationToken);

        return users is [{ Type: GithubUserType.User } user] && user.Id.Equals(userId)
            ? new GithubUserModel(user.Id, user.Username)
            : null;
    }

    public async Task<GithubRepositoryModel?> FindRepositoryByIdAsync(
        long repositoryId,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/repositories/{repositoryId}");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        HttpResponseMessage response = await _githubHttpClient.SendAsync(message, cancellationToken);

        if (IsValidResponse(response) is false)
            return null;

        return await response.Content.DeserializeAsync<GithubRepositoryModel>(_serializer, cancellationToken);
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
        {"permission":"{{permission.ToGithubApiString()}}"}
        """;

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        message.Content = new StringContent(content);

        HttpResponseMessage response = await _githubHttpClient.SendAsync(message, cancellationToken);

        if (response.IsSuccessStatusCode is false)
        {
            _logger.LogError(
                "Failed to update team permissions for organization = {OrganizationId}, team = {TeamId}, repository = {Owner}/{RepositoryName}, permission = {Permission}",
                organizationId,
                teamId,
                repositoryOwner,
                repositoryName,
                permission);
        }
    }

    private bool IsValidResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return true;

        if (response.StatusCode is HttpStatusCode.NotFound)
            return false;

        _logger.LogError(
            "Failed to execute github request = {RequestUri}, response code = {ResponseCode}",
            response.RequestMessage?.RequestUri,
            response.StatusCode);

        return false;
    }
}