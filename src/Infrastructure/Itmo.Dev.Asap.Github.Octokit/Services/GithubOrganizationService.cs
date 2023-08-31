using Itmo.Dev.Asap.Github.Application.Octokit.Clients;
using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Octokit.Clients;
using Octokit;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

internal class GithubOrganizationService : IGithubOrganizationService
{
    private readonly IGithubClientProvider _clientProvider;
    private readonly GithubApiClient _apiClient;

    public GithubOrganizationService(
        IGithubClientProvider clientProvider,
        GithubApiClient apiClient)
    {
        _clientProvider = clientProvider;
        _apiClient = apiClient;
    }

    public async Task<GithubOrganizationModel?> FindByIdAsync(long organizationId, CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetClientAsync(cancellationToken);
        string token = client.Connection.Credentials.GetToken();

        return await _apiClient.FindOrganizationByIdAsync(organizationId, token, cancellationToken);
    }

    public async Task<GithubTeamModel?> FindTeamAsync(
        long organizationId,
        long teamId,
        CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetOrganizationClientAsync(organizationId, cancellationToken);
        string token = client.Connection.Credentials.GetToken();

        return await _apiClient.FindTeamByIdAsync(organizationId, teamId, token, cancellationToken);
    }

    public async Task<IReadOnlyCollection<GithubUserModel>> GetTeamMembersAsync(
        long organizationId,
        long teamId,
        CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetOrganizationClientAsync(organizationId, cancellationToken);
        IReadOnlyList<User> teamMembers = await client.Organization.Team.GetAllMembers((int)teamId);

        return teamMembers.Select(u => new GithubUserModel(u.Id, u.Login)).ToArray();
    }
}