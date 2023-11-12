using FluentChaining;
using Itmo.Dev.Asap.Github.Caching.Tools;
using Itmo.Dev.Asap.Github.Common.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Github.Caching.ProviderConfiguration;

public class InMemoryProviderConfigurationLink : ILink<ConfigurationCommand>
{
    public Unit Process(
        ConfigurationCommand request,
        SynchronousContext context,
        LinkDelegate<ConfigurationCommand, SynchronousContext, Unit> next)
    {
        const string key = "Infrastructure:Cache:Provider:InMemory:Enabled";

        bool isEnabled = request.Configuration.GetSection(key).Get<bool>();

        if (isEnabled is false)
            return next.Invoke(request, context);

        request.Collection.AddSingleton<IGithubCache, GithubMemoryCache>();

        return Unit.Value;
    }
}