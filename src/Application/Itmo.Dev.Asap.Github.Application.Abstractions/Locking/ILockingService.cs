namespace Itmo.Dev.Asap.Github.Application.Abstractions.Locking;

public interface ILockingService
{
    ValueTask<ILockHandle> AcquireAsync(object key, CancellationToken cancellationToken);
}