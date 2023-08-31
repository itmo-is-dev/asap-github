using FluentSerialization;
using Itmo.Dev.Asap.Github.Application.Octokit.Models;

namespace Itmo.Dev.Asap.Github.Octokit.Tools;

public class GithubSerializationConfiguration : ISerializationConfiguration
{
    public void Configure(ISerializationConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Type<GithubUserModel>(builder =>
        {
            builder.Property(x => x.Id).Called("id");
            builder.Property(x => x.Username).Called("login");
        });

        configurationBuilder.Type<GithubOrganizationModel>(builder =>
        {
            builder.Property(x => x.Id).Called("id");
            builder.Property(x => x.Name).Called("login");
        });

        configurationBuilder.Type<GithubRepositoryModel>(builder =>
        {
            builder.Property(x => x.Id).Called("id");
            builder.Property(x => x.Name).Called("name");
        });

        configurationBuilder.Type<GithubTeamModel>(builder =>
        {
            builder.Property(x => x.Id).Called("id");
            builder.Property(x => x.Name).Called("slug");
        });
    }
}