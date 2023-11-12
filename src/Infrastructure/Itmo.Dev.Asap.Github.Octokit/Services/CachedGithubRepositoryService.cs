using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Common.Tools;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

internal class CachedGithubRepositoryService : IGithubRepositoryService
{
    private readonly IGithubCache _cache;
    private readonly IGithubRepositoryService _service;

    public CachedGithubRepositoryService(IGithubCache cache, IGithubRepositoryService service)
    {
        _cache = cache;
        _service = service;
    }

    public Task<GithubRepositoryModel?> FindByIdAsync(
        long organizationId,
        long repositoryId,
        CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            $"{nameof(CachedGithubRepositoryService)}-{nameof(FindByIdAsync)}-{organizationId}-{repositoryId}",
            cancellationToken,
            c => _service.FindByIdAsync(organizationId, repositoryId, c));
    }

    public Task AddTeamPermissionAsync(
        long organizationId,
        long repositoryId,
        long teamId,
        RepositoryPermission permission,
        CancellationToken cancellationToken)
    {
        return _service.AddTeamPermissionAsync(organizationId, repositoryId, teamId, permission, cancellationToken);
    }

    public Task<long?> CreateRepositoryFromTemplateAsync(
        long organizationId,
        string newRepositoryName,
        long templateRepositoryId,
        CancellationToken cancellationToken)
    {
        return _service.CreateRepositoryFromTemplateAsync(
            organizationId,
            newRepositoryName,
            templateRepositoryId,
            cancellationToken);
    }

    public Task<AddPermissionResult> AddUserPermissionAsync(
        long organizationId,
        long repositoryId,
        long userId,
        RepositoryPermission permission,
        CancellationToken cancellationToken)
    {
        return _service.AddUserPermissionAsync(organizationId, repositoryId, userId, permission, cancellationToken);
    }
}