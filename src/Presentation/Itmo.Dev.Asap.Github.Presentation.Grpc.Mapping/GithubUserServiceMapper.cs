using Itmo.Dev.Asap.Github.Application.Contracts.Users.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries;
using Itmo.Dev.Asap.Github.Application.Dto.Users;
using Itmo.Dev.Asap.Github.Models;
using Itmo.Dev.Asap.Github.Users;
using Riok.Mapperly.Abstractions;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;

[Mapper]
internal static partial class GithubUserServiceMapper
{
    public static partial FindUsersByIds.Query MapTo(this FindByIdsRequest request);

    public static partial UpdateGithubUsernames.Command MapTo(this UpdateUsernameRequest request);

    public static FindByIdsResponse MapFrom(this FindUsersByIds.Response response)
    {
        IEnumerable<GithubUser> users = response.Users.Select(MapToGithubUsers);
        return new FindByIdsResponse { Users = { users } };
    }

    [MapProperty(nameof(GithubUserDto.Id), nameof(GithubUser.UserId))]
    [MapProperty(nameof(GithubUserDto.GithubUsername), nameof(GithubUser.Username))]
    private static partial GithubUser MapToGithubUsers(GithubUserDto user);
}