using Itmo.Dev.Asap.Github.Caching.Models;
using Itmo.Dev.Asap.Github.Caching.Tools;
using Itmo.Dev.Asap.Github.Common.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Itmo.Dev.Asap.Github.Caching.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGithubCaching(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddOptions<GithubCacheConfiguration>().BindConfiguration("Infrastructure:Cache");

        const string key = "Infrastructure:Cache:Redis";

        RedisConfiguration? redisConfig = configuration.GetSection(key).Get<RedisConfiguration>();

        if (redisConfig is null)
            throw new InvalidOperationException("Redis is not configured");

        collection.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = new ConfigurationOptions
            {
                ServiceName = redisConfig.ServiceName,
                ClientName = redisConfig.ClientName,
                Password = redisConfig.Password,
                AllowAdmin = true,
            };

            foreach (string endpoint in redisConfig.Endpoints)
            {
                options.ConfigurationOptions.EndPoints.Add(endpoint);
            }
        });

        collection.AddSingleton<IGithubCache, GithubRedisCache>();

        return collection;
    }
}