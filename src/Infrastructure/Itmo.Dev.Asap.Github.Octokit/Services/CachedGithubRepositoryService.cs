using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Common.Tools;
using Microsoft.Extensions.Caching.Memory;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

internal class CachedGithubRepositoryService : IGithubRepositoryService
{
    private readonly IGithubMemoryCache _cache;
    private readonly IGithubRepositoryService _service;

    public CachedGithubRepositoryService(IGithubMemoryCache cache, IGithubRepositoryService service)
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
            (nameof(CachedGithubRepositoryService), nameof(FindByIdAsync), organizationId, repositoryId),
            _ => _service.FindByIdAsync(organizationId, repositoryId, cancellationToken));
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