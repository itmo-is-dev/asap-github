using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Common.Tools;
using Microsoft.Extensions.Caching.Memory;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

internal class CachedGithubOrganizationService : IGithubOrganizationService
{
    private readonly IGithubMemoryCache _cache;
    private readonly IGithubOrganizationService _service;

    public CachedGithubOrganizationService(IGithubMemoryCache cache, IGithubOrganizationService service)
    {
        _cache = cache;
        _service = service;
    }

    public Task<GithubOrganizationModel?> FindByIdAsync(long organizationId, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            (nameof(CachedGithubOrganizationService), nameof(FindByIdAsync), organizationId),
            _ => _service.FindByIdAsync(organizationId, cancellationToken));
    }

    public Task<GithubTeamModel?> FindTeamAsync(long organizationId, long teamId, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            (nameof(CachedGithubOrganizationService), nameof(FindTeamAsync), organizationId, teamId),
            _ => _service.FindTeamAsync(organizationId, teamId, cancellationToken));
    }

    public async Task<IReadOnlyCollection<GithubUserModel>> GetTeamMembersAsync(
        long organizationId,
        long teamId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<GithubUserModel>? members = await _cache.GetOrCreateAsync(
            (nameof(CachedGithubOrganizationService), nameof(GetTeamMembersAsync), organizationId, teamId),
            _ => _service.GetTeamMembersAsync(organizationId, teamId, cancellationToken));

        return members ?? Array.Empty<GithubUserModel>();
    }
}