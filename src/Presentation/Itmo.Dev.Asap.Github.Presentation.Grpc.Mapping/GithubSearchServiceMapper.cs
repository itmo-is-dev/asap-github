using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Models;
using Riok.Mapperly.Abstractions;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;

[Mapper]
public static partial class GithubSearchServiceMapper
{
    public static partial GithubOrganization MapFrom(this GithubOrganizationModel model);

    public static partial GithubRepository MapFrom(this GithubRepositoryModel model);

    public static partial GithubTeam MapFrom(this GithubTeamModel model);
}