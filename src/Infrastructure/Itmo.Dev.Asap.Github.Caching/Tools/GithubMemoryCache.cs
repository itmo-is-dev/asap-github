using Itmo.Dev.Asap.Github.Caching.Models;
using Itmo.Dev.Asap.Github.Common.Tools;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Github.Caching.Tools;

public sealed class GithubMemoryCache : IGithubCache, IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _entryOptions;

    public GithubMemoryCache(IOptions<GithubCacheConfiguration> options)
    {
        var memoryCacheOptions = new MemoryCacheOptions
        {
            SizeLimit = options.Value.SizeLimit,
            ExpirationScanFrequency = options.Value.ExpirationScanFrequency,
        };

        _cache = new MemoryCache(memoryCacheOptions);

        _entryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.Value.EntryAbsoluteExpiration,
            SlidingExpiration = options.Value.EntrySlidingExpiration,
        };
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        CancellationToken cancellationToken,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpirationRelativeToNow = null,
        TimeSpan? slidingExpiration = null)
    {
        if (_cache.TryGetValue(key, out T? value) && value is not null)
        {
            return value;
        }

        value = await factory.Invoke(cancellationToken);

        absoluteExpirationRelativeToNow ??= _entryOptions.AbsoluteExpirationRelativeToNow;
        slidingExpiration ??= _entryOptions.SlidingExpiration;

        using ICacheEntry entry = _cache.CreateEntry(key);
        entry.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
        entry.SlidingExpiration = slidingExpiration;
        entry.Value = value;

        return value;
    }

    public void Dispose()
    {
        _cache.Dispose();
    }
}