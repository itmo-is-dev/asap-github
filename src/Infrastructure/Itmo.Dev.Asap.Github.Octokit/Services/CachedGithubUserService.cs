using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Common.Tools;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

public class CachedGithubUserService : IGithubUserService
{
    private readonly IGithubCache _cache;
    private readonly IGithubUserService _service;

    public CachedGithubUserService(IGithubCache cache, IGithubUserService service)
    {
        _cache = cache;
        _service = service;
    }

    public Task<GithubUserModel?> FindByIdAsync(long userId, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            $"{nameof(CachedGithubUserService)}-{nameof(FindByIdAsync)}-{userId}",
            cancellationToken,
            c => _service.FindByIdAsync(userId, c),
            absoluteExpirationRelativeToNow: TimeSpan.FromHours(24),
            slidingExpiration: TimeSpan.FromHours(10));
    }

    public Task<GithubUserModel?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            $"{nameof(CachedGithubUserService)}-{nameof(FindByUsernameAsync)}-{username}",
            cancellationToken,
            c => _service.FindByUsernameAsync(username, c),
            absoluteExpirationRelativeToNow: TimeSpan.FromHours(24),
            slidingExpiration: TimeSpan.FromHours(10));
    }
}