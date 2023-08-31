using Itmo.Dev.Asap.Github.Application.Octokit.Models;

namespace Itmo.Dev.Asap.Github.Application.Octokit.Services;

public interface IGithubRepositoryService
{
    Task<GithubRepositoryModel?> FindByIdAsync(
        long organizationId,
        long repositoryId,
        CancellationToken cancellationToken);

    Task AddTeamPermissionAsync(
        long organizationId,
        long repositoryId,
        long teamId,
        RepositoryPermission permission,
        CancellationToken cancellationToken);

    Task<long?> CreateRepositoryFromTemplateAsync(
        long organizationId,
        string newRepositoryName,
        long templateRepositoryId,
        CancellationToken cancellationToken);

    Task<AddPermissionResult> AddUserPermissionAsync(
        long organizationId,
        long repositoryId,
        long userId,
        RepositoryPermission permission,
        CancellationToken cancellationToken);
}