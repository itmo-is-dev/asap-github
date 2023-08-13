using GitHubJwt;
using Itmo.Dev.Asap.Github.Application.Octokit.Clients;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Octokit.Clients;
using Itmo.Dev.Asap.Github.Octokit.Configuration;
using Itmo.Dev.Asap.Github.Octokit.Configuration.ServiceClients;
using Itmo.Dev.Asap.Github.Octokit.CredentialStores;
using Itmo.Dev.Asap.Github.Octokit.Services;
using Itmo.Dev.Asap.Github.Octokit.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Octokit;

namespace Itmo.Dev.Asap.Github.Octokit.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOctokitIntegration(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        collection.AddOptions<GithubOctokitConfiguration>().BindConfiguration("Infrastructure:Octokit");
        collection.AddSingleton<IValidateOptions<GithubOctokitConfiguration>, GithubOctokitConfiguration>();

        collection.AddGithubClients(configuration);
        collection.AddGithubServices();

        return collection;
    }

    private static IServiceCollection AddGithubClients(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddSingleton(provider =>
        {
            IOptions<GithubOctokitConfiguration> octokitConfiguration = provider
                .GetRequiredService<IOptions<GithubOctokitConfiguration>>();

            var privateKeySource = new FullStringPrivateKeySource(octokitConfiguration.Value.PrivateKey);

            var jwtFactoryOptions = new GitHubJwtFactoryOptions
            {
                // The GitHub App Id
                AppIntegrationId = octokitConfiguration.Value.AppId,

                // 10 minutes is the maximum time allowed
                ExpirationSeconds = octokitConfiguration.Value.JwtExpirationSeconds,
            };

            return new GitHubJwtFactory(privateKeySource, jwtFactoryOptions);
        });

        collection.AddSingleton<IGitHubClient>(serviceProvider =>
        {
            GitHubJwtFactory githubJwtFactory = serviceProvider.GetService<GitHubJwtFactory>()!;

            return new GitHubClient(
                new ProductHeaderValue("Itmo.Dev.Asap"),
                new GithubAppCredentialStore(githubJwtFactory));
        });

        FluentChaining.IChain<ServiceClientCommand> serviceClientChain = FluentChaining.FluentChaining
            .CreateChain<ServiceClientCommand>(start => start
                .Then<InstallationServiceClientLink>()
                .Then<OrganizationServiceClientLink>()
                .Then<UserServiceClientLink>()
                .FinishWith(() => throw new InvalidOperationException("Please configure Infrastructure:Octokit:Service")));

        serviceClientChain.Process(new ServiceClientCommand(collection, configuration));

        collection.AddSingleton<IGithubClientProvider, GithubClientProvider>();

        return collection;
    }

    private static IServiceCollection AddGithubServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IGithubUserService, GithubUserService>()
            .AddScoped<IGithubOrganizationService, GithubOrganizationService>()
            .AddScoped<IGithubRepositoryService, GithubRepositoryService>();
    }
}