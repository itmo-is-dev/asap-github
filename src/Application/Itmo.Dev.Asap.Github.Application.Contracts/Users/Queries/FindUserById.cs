using Itmo.Dev.Asap.Github.Application.Models.Users;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries;

public static class FindUserById
{
    public record Query(Guid Id) : IRequest<Response>;

#pragma warning disable CA1724
    public record Response(EnrichedGithubUser? User);
}