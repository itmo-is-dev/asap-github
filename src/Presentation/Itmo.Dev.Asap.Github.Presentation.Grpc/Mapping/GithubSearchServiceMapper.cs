using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Models;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;

public static class GithubSearchServiceMapper
{
    public static GithubOrganization MapFrom(this GithubOrganizationModel model)
    {
        return new GithubOrganization
        {
            Id = model.Id,
            Name = model.Name,
            AvatarUrl = model.AvatarUrl,
        };
    }

    public static GithubRepository MapFrom(this GithubRepositoryModel model)
    {
        return new GithubRepository
        {
            Id = model.Id,
            Name = model.Name,
        };
    }

    public static GithubTeam MapFrom(this GithubTeamModel model)
    {
        return new GithubTeam
        {
            Id = model.Id,
            Name = model.Name,
        };
    }
}