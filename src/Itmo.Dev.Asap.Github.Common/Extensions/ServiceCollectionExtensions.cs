using Itmo.Dev.Asap.Github.Common.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itmo.Dev.Asap.Github.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommon(this IServiceCollection collection)
    {
        collection.AddSingleton(p =>
        {
            IOptions<GithubSerializerOptions> options = p.GetRequiredService<IOptions<GithubSerializerOptions>>();
            return JsonSerializer.Create(options.Value.Settings);
        });

        return collection;
    }
}