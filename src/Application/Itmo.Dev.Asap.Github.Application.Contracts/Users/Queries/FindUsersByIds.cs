using Itmo.Dev.Asap.Github.Application.Dto.Users;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries;

public static class FindUsersByIds
{
    public record Query(IEnumerable<Guid> UserIds) : IRequest<Response>;

    public record Response(IReadOnlyCollection<GithubUserDto> Users);
}