using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.Enrichment;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries.FindUsersByIds;

namespace Itmo.Dev.Asap.Github.Application.Users;

internal class FindUsersByIdsHandler : IRequestHandler<Query, Response>
{
    private readonly IPersistenceContext _context;
    private readonly IGithubUserEnricher _enricher;

    public FindUsersByIdsHandler(IPersistenceContext context, IGithubUserEnricher enricher)
    {
        _context = context;
        _enricher = enricher;
    }

    public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = GithubUserQuery.Build(x => x.WithIds(request.UserIds));

        EnrichedGithubUser[] dto = await _context.Users
            .QueryAsync(query, cancellationToken)
            .SelectAwait(async x => await _enricher.EnrichAsync(x, cancellationToken))
            .ToArrayAsync(cancellationToken);

        return new Response(dto);
    }
}