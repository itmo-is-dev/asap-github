using FluentChaining;
using Itmo.Dev.Asap.Github.Caching.Tools;
using Itmo.Dev.Asap.Github.Common.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Itmo.Dev.Asap.Github.Caching.ProviderConfiguration;

public class RedisProviderConfigurationLink : ILink<ConfigurationCommand>
{
    public Unit Process(
        ConfigurationCommand request,
        SynchronousContext context,
        LinkDelegate<ConfigurationCommand, SynchronousContext, Unit> next)
    {
        const string key = "Infrastructure:Cache:Provider:Redis";
        const string enabledKey = $"{key}:Enabled";
        const string configurationKey = $"{key}:Configuration";

        bool isEnabled = request.Configuration.GetSection(enabledKey).Get<bool>();

        if (isEnabled is false)
            return next.Invoke(request, context);

        ConfigurationOptions? redisConfig = request.Configuration.GetSection(key).Get<ConfigurationOptions>();

        if (redisConfig is null)
            return next.Invoke(request, context);

        request.Collection.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = request.Configuration.GetSection(configurationKey).Value;
            options.ConfigurationOptions = redisConfig;
        });
        request.Collection.AddSingleton<IGithubCache, GithubRedisCache>();

        return Unit.Value;
    }
}