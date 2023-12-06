using Itmo.Dev.Asap.Github.Application.Abstractions.Locking;

namespace Itmo.Dev.Asap.Infrastructure.Locking.Models;

internal sealed class LockHandle : ILockHandle
{
    private readonly SemaphoreWrapper _semaphore;

    public LockHandle(SemaphoreWrapper semaphore)
    {
        _semaphore = semaphore;
    }

    public ValueTask DisposeAsync()
    {
        _semaphore.Semaphore.Release();
        return ValueTask.CompletedTask;
    }
}