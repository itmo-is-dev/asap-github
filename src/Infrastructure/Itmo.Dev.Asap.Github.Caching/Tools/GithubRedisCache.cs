using Itmo.Dev.Asap.Github.Caching.Models;
using Itmo.Dev.Asap.Github.Common.Tools;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itmo.Dev.Asap.Github.Caching.Tools;

public class GithubRedisCache : IGithubCache
{
    private readonly IDistributedCache _cache;
    private readonly GithubCacheConfiguration _configuration;
    private readonly JsonSerializer _serializer;
    private readonly ILogger<GithubRedisCache> _logger;

    public GithubRedisCache(
        IDistributedCache cache,
        IOptions<GithubCacheConfiguration> configuration,
        JsonSerializer serializer,
        ILogger<GithubRedisCache> logger)
    {
        _cache = cache;
        _serializer = serializer;
        _logger = logger;
        _configuration = configuration.Value;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        CancellationToken cancellationToken,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpirationRelativeToNow = null,
        TimeSpan? slidingExpiration = null)
    {
        byte[]? existingData = await _cache.GetAsync(key, cancellationToken);

        if (existingData is not null)
        {
            using var stream = new MemoryStream(existingData);
            using var streamReader = new StreamReader(stream);
            await using var jsonReader = new JsonTextReader(streamReader);

            try
            {
                T? value = _serializer.Deserialize<T>(jsonReader);

                if (value is not null)
                    return value;
            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e, "Failed to deserialize cached response");
            }
        }

        {
            T value = await factory.Invoke(cancellationToken);

            using var stream = new MemoryStream();
            await using var streamWriter = new StreamWriter(stream);

            await using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                _serializer.Serialize(jsonWriter, value);
            }

            absoluteExpirationRelativeToNow ??= _configuration.EntryAbsoluteExpiration;
            slidingExpiration ??= _configuration.EntrySlidingExpiration;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow,
                SlidingExpiration = slidingExpiration,
            };

            await _cache.SetAsync(key, stream.ToArray(), options, cancellationToken);

            return value;
        }
    }
}