using Microsoft.Extensions.Caching.Memory;

namespace Itmo.Dev.Asap.Github.Common.Extensions;

public static class MemoryCacheExtensions
{
    public static T GetOrCreateNotNull<T>(this IMemoryCache cache, object key, Func<ICacheEntry, T> factory)
    {
        if (cache.TryGetValue(key, out T? value) && value is not null)
            return value;

        using ICacheEntry entry = cache.CreateEntry(key);

        value = factory.Invoke(entry);
        entry.Value = value;

        return value;
    }
}