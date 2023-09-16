using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Common.Extensions;
using Itmo.Dev.Asap.Github.Common.Tools;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

public class CachedGithubUserService : IGithubUserService
{
    private readonly IGithubMemoryCache _cache;
    private readonly IGithubUserService _service;

    public CachedGithubUserService(IGithubMemoryCache cache, IGithubUserService service)
    {
        _cache = cache;
        _service = service;
    }

    public Task<GithubUserModel?> FindByIdAsync(long userId, CancellationToken cancellationToken)
    {
        object key = (nameof(CachedGithubUserService), nameof(FindByIdAsync), userId);

        return _cache.GetOrCreateAsync(
            key,
            () => _service.FindByIdAsync(userId, cancellationToken),
            absoluteExpirationRelativeToNow: TimeSpan.FromHours(24),
            slidingExpiration: TimeSpan.FromHours(10));
    }

    public Task<GithubUserModel?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        object key = (nameof(CachedGithubUserService), nameof(FindByUsernameAsync), username);

        return _cache.GetOrCreateAsync(
            key,
            () => _service.FindByUsernameAsync(username, cancellationToken),
            absoluteExpirationRelativeToNow: TimeSpan.FromHours(24),
            slidingExpiration: TimeSpan.FromHours(10));
    }
}