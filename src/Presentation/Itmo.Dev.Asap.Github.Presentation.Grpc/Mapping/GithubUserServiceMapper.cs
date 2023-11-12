using Itmo.Dev.Asap.Github.Application.Contracts.Users.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using Itmo.Dev.Asap.Github.Common.Tools;
using Itmo.Dev.Asap.Github.Users;
using GithubUser = Itmo.Dev.Asap.Github.Models.GithubUser;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;

internal static class GithubUserServiceMapper
{
    public static FindUsersByIds.Query MapTo(this FindByIdsRequest request)
        => new FindUsersByIds.Query(request.UserIds.Select(x => x.ToGuid()));

    public static UpdateGithubUsernames.Command MapTo(this UpdateUsernameRequest request)
        => new UpdateGithubUsernames.Command(request.Models.Select(MapTo).ToArray());

    public static FindByIdsResponse MapFrom(this FindUsersByIds.Response response)
    {
        IEnumerable<GithubUser> users = response.Users.Select(MapToGithubUsers);
        return new FindByIdsResponse { Users = { users } };
    }

    private static GithubUser MapToGithubUsers(EnrichedGithubUser user)
    {
        return new GithubUser
        {
            UserId = user.Id.ToString(),
            GithubId = user.GithubId,
            Username = user.GithubUsername,
        };
    }

    private static UpdateGithubUsernames.Command.Model MapTo(UpdateUsernameRequest.Types.Model model)
        => new UpdateGithubUsernames.Command.Model(model.UserId.ToGuid(), model.GithubUsername);
}