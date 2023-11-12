using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Github.Caching.ProviderConfiguration;

public record ConfigurationCommand(IConfiguration Configuration, IServiceCollection Collection);