using Itmo.Dev.Asap.Application.Abstractions.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Github.Application.Mapping.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMapping(this IServiceCollection collection)
    {
        collection.AddScoped<IGithubUserMapper, GithubUserMapper>();
        return collection;
    }
}