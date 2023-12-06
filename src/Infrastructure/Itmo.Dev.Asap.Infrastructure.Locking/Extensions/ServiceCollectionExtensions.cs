using Itmo.Dev.Asap.Github.Application.Abstractions.Locking;
using Itmo.Dev.Asap.Infrastructure.Locking.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Infrastructure.Locking.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLocking(this IServiceCollection collection)
    {
        return collection.AddSingleton<ILockingService, LockingService>();
    }
}