using Itmo.Dev.Asap.Github.Caching.Models;
using Itmo.Dev.Asap.Github.Caching.Tools;
using Itmo.Dev.Asap.Github.Common.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Github.Caching.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGithubCaching(this IServiceCollection collection)
    {
        collection.AddOptions<GithubCacheConfiguration>().BindConfiguration("Infrastructure:Cache");
        collection.AddSingleton<IGithubMemoryCache, GithubMemoryCache>();

        return collection;
    }
}