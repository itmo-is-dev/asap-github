using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Github.Octokit.Configuration.ServiceClients;

public record ServiceClientCommand(IServiceCollection ServiceCollection, IConfiguration Configuration);