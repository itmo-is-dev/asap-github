using Itmo.Dev.Asap.Github.Application.Dto.Users;
using Itmo.Dev.Asap.Github.Domain.Users;

namespace Itmo.Dev.Asap.Github.Application.Mapping;

public static class GithubUserMapping
{
    public static GithubUserDto ToDto(this GithubUser user)
        => new GithubUserDto(user.Id, user.Username);
}