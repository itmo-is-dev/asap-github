using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Common.Extensions;
using Itmo.Dev.Asap.Github.Common.Tools;

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
        object key = (nameof(CachedGithubOrganizationService), nameof(FindByIdAsync), organizationId);

        return _cache.GetOrCreateAsync(
            key,
            () => _service.FindByIdAsync(organizationId, cancellationToken),
            absoluteExpirationRelativeToNow: TimeSpan.FromHours(24),
            slidingExpiration: TimeSpan.FromHours(10));
    }

    public Task<GithubTeamModel?> FindTeamAsync(long organizationId, long teamId, CancellationToken cancellationToken)
    {
        object key = (nameof(CachedGithubOrganizationService), nameof(FindTeamAsync), organizationId, teamId);

        return _cache.GetOrCreateAsync(
            key,
            () => _service.FindTeamAsync(organizationId, teamId, cancellationToken),
            absoluteExpirationRelativeToNow: TimeSpan.FromHours(24),
            slidingExpiration: TimeSpan.FromHours(10));
    }

    public async Task<IReadOnlyCollection<GithubUserModel>> GetTeamMembersAsync(
        long organizationId,
        long teamId,
        CancellationToken cancellationToken)
    {
        object key = (nameof(CachedGithubOrganizationService), nameof(GetTeamMembersAsync), organizationId, teamId);

        IReadOnlyCollection<GithubUserModel>? members = await _cache.GetOrCreateAsync(
            key,
            () => _service.GetTeamMembersAsync(organizationId, teamId, cancellationToken),
            absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(10),
            slidingExpiration: TimeSpan.FromSeconds(10));

        return members ?? Array.Empty<GithubUserModel>();
    }
}