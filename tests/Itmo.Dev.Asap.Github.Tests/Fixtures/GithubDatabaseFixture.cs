using Itmo.Dev.Asap.Github.DataAccess.Extensions;
using Itmo.Dev.Platform.Testing.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Github.Tests.Fixtures;

public class GithubDatabaseFixture : DatabaseFixture
{
    public ILogger<T> CreateLogger<T>()
    {
        ILoggerFactory factory = Provider.GetRequiredService<ILoggerFactory>();
        return factory.CreateLogger<T>();
    }

    protected override void ConfigureServices(IServiceCollection collection)
    {
        var configurationValues = new Dictionary<string, string?>
        {
            { "Infrastructure:DataAccess:PostgresConfiguration:Host", Container.Hostname },
            { "Infrastructure:DataAccess:PostgresConfiguration:Port", Container.GetMappedPublicPort(5432).ToString() },
            { "Infrastructure:DataAccess:PostgresConfiguration:Database", "postgres" },
            { "Infrastructure:DataAccess:PostgresConfiguration:Username", "postgres" },
            { "Infrastructure:DataAccess:PostgresConfiguration:Password", "postgres" },
            { "Infrastructure:DataAccess:PostgresConfiguration:SslMode", "Prefer" },
        };

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        collection.AddSingleton<IConfiguration>(configuration);
        collection.AddDataAccess();
    }

    protected override async ValueTask UseProviderAsync(IServiceProvider provider)
    {
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        await scope.UseDataAccessAsync(default);
    }
}