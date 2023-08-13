using Octokit;

namespace Itmo.Dev.Asap.Github.Application.Octokit.Services;

public interface IGithubOrganizationService
{
    Task<IReadOnlyCollection<string>> GetTeamMemberUsernamesAsync(
        string organizationName,
        string teamName,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetRepositoriesAsync(string organizationName, CancellationToken cancellationToken);

    Task<Team> GetTeamAsync(string organizationName, string teamName, CancellationToken cancellationToken);
}