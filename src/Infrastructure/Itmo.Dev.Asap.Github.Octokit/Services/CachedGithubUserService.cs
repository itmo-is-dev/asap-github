using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Common.Tools;
using Microsoft.Extensions.Caching.Memory;

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

    public async Task<GithubUserModel?> FindByIdAsync(long userId, CancellationToken cancellationToken)
    {
        object key = (nameof(CachedGithubUserService), nameof(FindByIdAsync), userId);

        if (_cache.TryGetValue(key, out GithubUserModel? value))
        {
            return value;
        }

        value = await _service.FindByIdAsync(userId, cancellationToken);

        ICacheEntry entry = _cache.CreateEntry(key);
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2);
        entry.Value = value;

        return value;
    }

    public Task<GithubUserModel?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            (nameof(CachedGithubUserService), nameof(FindByUsernameAsync), username),
            _ => _service.FindByUsernameAsync(username, cancellationToken));
    }
}