using Itmo.Dev.Asap.Application.Abstractions.Mapping;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Dto.Users;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries.FindUsersByIds;

namespace Itmo.Dev.Asap.Github.Application.Handlers.Users;

internal class FindUsersByIdsHandler : IRequestHandler<Query, Response>
{
    private readonly IPersistenceContext _context;
    private readonly IGithubUserMapper _mapper;

    public FindUsersByIdsHandler(IPersistenceContext context, IGithubUserMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = GithubUserQuery.Build(x => x.WithIds(request.UserIds));

        GithubUserDto[] dto = await _context.Users
            .QueryAsync(query, cancellationToken)
            .SelectAwait(async x => await _mapper.MapAsync(x, cancellationToken))
            .ToArrayAsync(cancellationToken);

        return new Response(dto);
    }
}