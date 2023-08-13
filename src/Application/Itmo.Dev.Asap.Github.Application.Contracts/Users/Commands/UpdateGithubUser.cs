using Itmo.Dev.Asap.Github.Application.Dto.Users;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Users.Commands;

public static class UpdateGithubUser
{
    public record Command(Guid UserId, string GithubUsername) : IRequest<Response>;

    public record Response(GithubUserDto User);
}