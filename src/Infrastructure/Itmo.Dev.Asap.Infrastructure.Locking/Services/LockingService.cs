using Itmo.Dev.Asap.Github.Application.Abstractions.Locking;
using Itmo.Dev.Asap.Infrastructure.Locking.Models;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.Infrastructure.Locking.Services;

public class LockingService : ILockingService
{
    private readonly ConditionalWeakTable<object, SemaphoreWrapper> _table;

    public LockingService()
    {
        _table = new ConditionalWeakTable<object, SemaphoreWrapper>();
    }

    public async ValueTask<ILockHandle> AcquireAsync(object key, CancellationToken cancellationToken)
    {
        SemaphoreWrapper semaphore = _table.GetValue(
            key,
            _ => new SemaphoreWrapper(new SemaphoreSlim(1, 1)));

        await semaphore.Semaphore.WaitAsync(cancellationToken);

        return new LockHandle(semaphore);
    }
}