using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.Mapping;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries.FindSubjectCoursesByIds;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Handlers;

internal class FindSubjectCoursesByIdsHandler : IRequestHandler<Query, Response>
{
    private readonly IPersistenceContext _context;
    private readonly IGithubSubjectCourseEnricher _enricher;

    public FindSubjectCoursesByIdsHandler(IPersistenceContext context, IGithubSubjectCourseEnricher enricher)
    {
        _context = context;
        _enricher = enricher;
    }

    public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseQuery.Build(x => x.WithIds(request.SubjectCourseIds));

        EnrichedGithubSubjectCourse[] subjectCourses = await _context.SubjectCourses
            .QueryAsync(query, cancellationToken)
            .SelectAwait(async sc => await _enricher.MapAsync(sc, cancellationToken))
            .ToArrayAsync(cancellationToken);

        return new Response(subjectCourses);
    }
}