using Itmo.Dev.Asap.Github.Common.Tools;
using Microsoft.Extensions.Caching.Memory;

namespace Itmo.Dev.Asap.Github.Common.Extensions;

public static class GithubMemoryCacheExtensions
{
    public static async Task<T?> GetOrCreateAsync<T>(
        this IGithubMemoryCache cache,
        object key,
        Func<Task<T>> valueFactory,
        TimeSpan? absoluteExpirationRelativeToNow = null,
        TimeSpan? slidingExpiration = null)
    {
        if (cache.TryGetValue(key, out T? value))
        {
            return value;
        }

        value = await valueFactory.Invoke();

        ICacheEntry entry = cache.CreateEntry(key);
        entry.Value = value;
        entry.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
        entry.SlidingExpiration = slidingExpiration;

        return value;
    }
}