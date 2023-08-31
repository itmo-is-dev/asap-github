using Itmo.Dev.Asap.Github.Application.Octokit.Models;

namespace Itmo.Dev.Asap.Github.Application.Octokit.Services;

public interface IGithubOrganizationService
{
    Task<GithubOrganizationModel?> FindByIdAsync(long organizationId, CancellationToken cancellationToken);

    Task<GithubTeamModel?> FindTeamAsync(long organizationId, long teamId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<GithubUserModel>> GetTeamMembersAsync(
        long organizationId,
        long teamId,
        CancellationToken cancellationToken);
}