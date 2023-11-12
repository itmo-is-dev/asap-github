using FluentChaining;
using Itmo.Dev.Asap.Github.Caching.Models;
using Itmo.Dev.Asap.Github.Caching.ProviderConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Chain = FluentChaining.FluentChaining;

namespace Itmo.Dev.Asap.Github.Caching.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGithubCaching(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddOptions<GithubCacheConfiguration>().BindConfiguration("Infrastructure:Cache");

        IChain<ConfigurationCommand> configurationChain = Chain.CreateChain<ConfigurationCommand>(start => start
            .Then<RedisProviderConfigurationLink>()
            .Then<InMemoryProviderConfigurationLink>()
            .FinishWith(() => throw new InvalidOperationException("No cache provider selected")));

        var command = new ConfigurationCommand(configuration, collection);
        configurationChain.Process(command);

        return collection;
    }
}