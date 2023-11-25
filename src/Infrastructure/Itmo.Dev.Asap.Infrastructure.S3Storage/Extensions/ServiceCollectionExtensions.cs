using Itmo.Dev.Asap.Github.Application.Abstractions.Storage;
using Itmo.Dev.Asap.Infrastructure.S3Storage.Options;
using Itmo.Dev.Asap.Infrastructure.S3Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Infrastructure.S3Storage.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureS3Storage(this IServiceCollection collection)
    {
        collection
            .AddOptions<S3StorageOptions>()
            .BindConfiguration("Infrastructure:Storage:S3")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        collection.AddScoped<IStorageService>(sp =>
        {
            return sp.GetRequiredService<IOptionsMonitor<S3StorageOptions>>().CurrentValue.IsEnabled
                ? ActivatorUtilities.CreateInstance<S3StorageService>(sp)
                : ActivatorUtilities.CreateInstance<MockS3StorageService>(sp);
        });

        return collection;
    }
}