using Itmo.Dev.Asap.Github.Application.Octokit.Models;

namespace Itmo.Dev.Asap.Github.Application.Octokit.Services;

public interface IGithubSearchService
{
    IAsyncEnumerable<GithubOrganizationModel> SearchOrganizationsAsync(string query, CancellationToken cancellationToken);

    IAsyncEnumerable<GithubRepositoryModel> SearchOrganizationRepositoriesAsync(
        long organizationId,
        string query,
        CancellationToken cancellationToken);

    IAsyncEnumerable<GithubTeamModel> SearchOrganizationTeamsAsync(
        long organizationId,
        string query,
        CancellationToken cancellationToken);
}