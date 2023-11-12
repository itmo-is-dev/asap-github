using FluentSerialization;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Octokit.Models;

namespace Itmo.Dev.Asap.Github.Octokit.Tools;

public class GithubSerializationConfiguration : ISerializationConfiguration
{
    public void Configure(ISerializationConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Type<GithubUserSerializationModel>(builder =>
        {
            builder.Property(x => x.Id).Called("id");
            builder.Property(x => x.Username).Called("login");

            builder.Property(x => x.Type)
                .Called("type")
                .ConvertedWith(
                    x => x.ToString(),
                    s => Enum.TryParse(s, out GithubUserType type) ? type : GithubUserType.Unknown);
        });

        configurationBuilder.Type<GithubOrganizationModel>(builder =>
        {
            builder.Property(x => x.Id).Called("id");
            builder.Property(x => x.Name).Called("login");
            builder.Property(x => x.AvatarUrl).Called("avatar_url");
        });

        configurationBuilder.Type<GithubRepositoryModel>(builder =>
        {
            builder.Property(x => x.Id).Called("id");
            builder.Property(x => x.Name).Called("name");
            builder.Property(x => x.IsTemplate).Called("is_template");
        });

        configurationBuilder.Type<GithubTeamModel>(builder =>
        {
            builder.Property(x => x.Id).Called("id");
            builder.Property(x => x.Name).Called("slug");
        });
    }
}