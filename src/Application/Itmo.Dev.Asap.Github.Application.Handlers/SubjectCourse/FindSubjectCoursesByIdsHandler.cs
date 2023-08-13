using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Dto.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Mapping;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries.FindSubjectCoursesByIds;

namespace Itmo.Dev.Asap.Github.Application.Handlers.SubjectCourse;

internal class FindSubjectCoursesByIdsHandler : IRequestHandler<Query, Response>
{
    private readonly IPersistenceContext _context;

    public FindSubjectCoursesByIdsHandler(IPersistenceContext context)
    {
        _context = context;
    }

    public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseQuery.Build(x => x.WithIds(request.SubjectCourseIds));

        IAsyncEnumerable<GithubSubjectCourse> subjectCourses = _context.SubjectCourses
            .QueryAsync(query, cancellationToken);

        List<GithubSubjectCourseDto> dto = await subjectCourses
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);

        return new Response(dto);
    }
}