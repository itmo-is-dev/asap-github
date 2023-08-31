using Itmo.Dev.Asap.Application.Abstractions.Mapping;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Dto.Users;
using Itmo.Dev.Asap.Github.Domain.Users;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries.FindUserById;

namespace Itmo.Dev.Asap.Github.Application.Handlers.Users;

internal class FindUserByIdHandler : IRequestHandler<Query, Response>
{
    private readonly IPersistenceContext _context;
    private readonly IGithubUserMapper _mapper;

    public FindUserByIdHandler(IPersistenceContext context, IGithubUserMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = GithubUserQuery.Build(x => x.WithId(request.Id).WithLimit(1));

        GithubUser? user = await _context.Users
            .QueryAsync(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
            return new Response(null);

        GithubUserDto dto = await _mapper.MapAsync(user, cancellationToken);

        return new Response(dto);
    }
}