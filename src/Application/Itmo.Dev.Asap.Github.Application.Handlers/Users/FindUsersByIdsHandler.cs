using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Dto.Users;
using Itmo.Dev.Asap.Github.Application.Mapping;
using Itmo.Dev.Asap.Github.Domain.Users;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries.FindUsersByIds;

namespace Itmo.Dev.Asap.Github.Application.Handlers.Users;

internal class FindUsersByIdsHandler : IRequestHandler<Query, Response>
{
    private readonly IPersistenceContext _context;

    public FindUsersByIdsHandler(IPersistenceContext context)
    {
        _context = context;
    }

    public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = GithubUserQuery.Build(x => x.WithIds(request.UserIds));

        IAsyncEnumerable<GithubUser> users = _context.Users.QueryAsync(query, cancellationToken);
        List<GithubUserDto> dto = await users.Select(x => x.ToDto()).ToListAsync(cancellationToken);

        return new Response(dto);
    }
}