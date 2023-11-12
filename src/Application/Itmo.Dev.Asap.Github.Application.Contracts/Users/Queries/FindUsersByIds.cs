using Itmo.Dev.Asap.Github.Application.Models.Users;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries;

internal static class FindUsersByIds
{
    public record Query(IEnumerable<Guid> UserIds) : IRequest<Response>;

    public record Response(IReadOnlyCollection<EnrichedGithubUser> Users);
}