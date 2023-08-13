using Itmo.Dev.Asap.Github.Application.Dto.Users;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries;

public static class FindUserById
{
    public record Query(Guid Id) : IRequest<Response>;

    public record Response(GithubUserDto? User);
}