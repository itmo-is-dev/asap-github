namespace Itmo.Dev.Asap.Github.Caching.Models;

public class GithubCacheConfiguration
{
    public int? EntryAbsoluteExpirationSeconds { get; init; }

    public int? EntrySlidingExpirationSeconds { get; init; }

    public TimeSpan? EntryAbsoluteExpiration => EntryAbsoluteExpirationSeconds is null
        ? null
        : TimeSpan.FromSeconds(EntryAbsoluteExpirationSeconds.Value);

    public TimeSpan? EntrySlidingExpiration => EntrySlidingExpirationSeconds is null
        ? null
        : TimeSpan.FromSeconds(EntrySlidingExpirationSeconds.Value);
}