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

    public Task<GithubUserModel?> FindByIdAsync(long userId, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            (nameof(CachedGithubUserService), nameof(FindByIdAsync), userId),
            _ => _service.FindByIdAsync(userId, cancellationToken));
    }
}