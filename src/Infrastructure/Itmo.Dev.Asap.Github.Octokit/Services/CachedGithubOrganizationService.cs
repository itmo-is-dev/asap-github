using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Common.Tools;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

internal class CachedGithubOrganizationService : IGithubOrganizationService
{
    private readonly IGithubCache _cache;
    private readonly IGithubOrganizationService _service;

    public CachedGithubOrganizationService(IGithubCache cache, IGithubOrganizationService service)
    {
        _cache = cache;
        _service = service;
    }

    public Task<GithubOrganizationModel?> FindByIdAsync(long organizationId, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            $"{nameof(CachedGithubOrganizationService)}-{nameof(FindByIdAsync)}-{organizationId}",
            cancellationToken,
            c => _service.FindByIdAsync(organizationId, c),
            absoluteExpirationRelativeToNow: TimeSpan.FromHours(24),
            slidingExpiration: TimeSpan.FromHours(10));
    }

    public Task<GithubTeamModel?> FindTeamAsync(long organizationId, long teamId, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            $"{nameof(CachedGithubOrganizationService)}-{nameof(FindTeamAsync)}-{organizationId}-{teamId}",
            cancellationToken,
            c => _service.FindTeamAsync(organizationId, teamId, c),
            absoluteExpirationRelativeToNow: TimeSpan.FromHours(24),
            slidingExpiration: TimeSpan.FromHours(10));
    }

    public async Task<IReadOnlyCollection<GithubUserModel>> GetTeamMembersAsync(
        long organizationId,
        long teamId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<GithubUserModel> members = await _cache.GetOrCreateAsync(
            $"{nameof(CachedGithubOrganizationService)}-{nameof(GetTeamMembersAsync)}-{organizationId}-{teamId}",
            cancellationToken,
            c => _service.GetTeamMembersAsync(organizationId, teamId, c),
            absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(10),
            slidingExpiration: TimeSpan.FromSeconds(10));

        return members;
    }
}