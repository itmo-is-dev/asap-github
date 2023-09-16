using Itmo.Dev.Asap.Github.Application.Octokit.Clients;
using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Octokit.Clients;
using Itmo.Dev.Asap.Github.Octokit.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

internal class GithubSearchService : IGithubSearchService
{
    private readonly IGithubClientProvider _clientProvider;
    private readonly GithubOctokitConfiguration _configuration;
    private readonly GithubApiClient _apiClient;
    private readonly ILogger<GithubSearchService> _logger;

    public GithubSearchService(
        IGithubClientProvider clientProvider,
        IOptions<GithubOctokitConfiguration> configuration,
        GithubApiClient apiClient,
        ILogger<GithubSearchService> logger)
    {
        _clientProvider = clientProvider;
        _apiClient = apiClient;
        _logger = logger;
        _configuration = configuration.Value;
    }

    public async IAsyncEnumerable<GithubOrganizationModel> SearchOrganizationsAsync(
        string query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetServiceClientAsync(cancellationToken);

        var request = new SearchUsersRequest(query);
        int count = 0;
        int totalCount = 0;

        while (count < _configuration.MaxSearchResponseSize)
        {
            SearchUsersResult response = await client.Search.SearchUsers(request);
            totalCount += response.Items.Count;

            foreach (User user in response.Items)
            {
                if (user.Type is not AccountType.Organization)
                    continue;

                count++;
                yield return new GithubOrganizationModel(user.Id, user.Login, user.AvatarUrl);
            }

            if (totalCount >= response.TotalCount)
                yield break;

            request.Page++;
        }
    }

    public async IAsyncEnumerable<GithubRepositoryModel> SearchOrganizationRepositoriesAsync(
        long organizationId,
        string query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetOrganizationClientAsync(organizationId, cancellationToken);
        string token = client.Connection.Credentials.GetToken();

        GithubOrganizationModel? organization = await _apiClient
            .FindOrganizationByIdAsync(organizationId, token, cancellationToken);

        if (organization is null)
        {
            _logger.LogCritical(
                "Failed to load organization = {OrganizationId}, but somehow got it's client",
                organizationId);

            yield break;
        }

        query = $"org:{organization.Name} {query}";

        var request = new SearchRepositoriesRequest(query);
        int count = 0;
        int totalCount = 0;

        while (count < _configuration.MaxSearchResponseSize)
        {
            SearchRepositoryResult response = await client.Search.SearchRepo(request);
            totalCount += response.Items.Count;

            foreach (Repository repository in response.Items)
            {
                count++;
                yield return new GithubRepositoryModel(repository.Id, repository.Name, repository.IsTemplate);
            }

            if (totalCount >= response.TotalCount)
                yield break;

            request.Page++;
        }
    }

    public async IAsyncEnumerable<GithubTeamModel> SearchOrganizationTeamsAsync(
        long organizationId,
        string query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetOrganizationClientAsync(organizationId, cancellationToken);
        string token = client.Connection.Credentials.GetToken();

        GithubOrganizationModel? organization = await _apiClient
            .FindOrganizationByIdAsync(organizationId, token, cancellationToken);

        if (organization is null)
        {
            _logger.LogCritical(
                "Failed to load organization = {OrganizationId}, but somehow got it's client",
                organizationId);

            yield break;
        }

        IReadOnlyList<Team> teams = await client.Organization.Team.GetAll(organization.Name);

        foreach (Team team in teams)
        {
            if (team.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase) is false)
                continue;

            yield return new GithubTeamModel(team.Id, team.Name);
        }
    }
}