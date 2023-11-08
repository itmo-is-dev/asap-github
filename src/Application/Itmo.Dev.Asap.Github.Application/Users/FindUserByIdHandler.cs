using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.Enrichment;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries.FindUserById;

namespace Itmo.Dev.Asap.Github.Application.Users;

internal class FindUserByIdHandler : IRequestHandler<Query, Response>
{
    private readonly IPersistenceContext _context;
    private readonly IGithubUserEnricher _enricher;

    public FindUserByIdHandler(IPersistenceContext context, IGithubUserEnricher enricher)
    {
        _context = context;
        _enricher = enricher;
    }

    public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = GithubUserQuery.Build(x => x.WithId(request.Id).WithLimit(1));

        GithubUser? user = await _context.Users
            .QueryAsync(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
            return new Response(null);

        EnrichedGithubUser dto = await _enricher.EnrichAsync(user, cancellationToken);

        return new Response(dto);
    }
}