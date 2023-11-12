namespace Itmo.Dev.Asap.Github.Common.Tools;

public interface IGithubCache
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        CancellationToken cancellationToken,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpirationRelativeToNow = null,
        TimeSpan? slidingExpiration = null);
}