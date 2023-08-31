using Itmo.Dev.Asap.Application.Abstractions.Mapping;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Dto.SubjectCourses;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries.FindSubjectCoursesByIds;

namespace Itmo.Dev.Asap.Github.Application.Handlers.SubjectCourse;

internal class FindSubjectCoursesByIdsHandler : IRequestHandler<Query, Response>
{
    private readonly IPersistenceContext _context;
    private readonly IGithubSubjectCourseMapper _mapper;

    public FindSubjectCoursesByIdsHandler(IPersistenceContext context, IGithubSubjectCourseMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseQuery.Build(x => x.WithIds(request.SubjectCourseIds));

        GithubSubjectCourseDto[] subjectCourses = await _context.SubjectCourses
            .QueryAsync(query, cancellationToken)
            .SelectAwait(async sc => await _mapper.MapAsync(sc, cancellationToken))
            .ToArrayAsync(cancellationToken);

        return new Response(subjectCourses);
    }
}