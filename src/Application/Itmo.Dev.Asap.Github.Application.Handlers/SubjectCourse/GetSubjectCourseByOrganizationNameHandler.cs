using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Dto.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Mapping;
using Itmo.Dev.Asap.Github.Common.Exceptions.Entities;
using Itmo.Dev.Asap.Github.Common.Extensions;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries.GetSubjectCourseByOrganizationName;

namespace Itmo.Dev.Asap.Github.Application.Handlers.SubjectCourse;

internal class GetSubjectCourseByOrganizationNameHandler : IRequestHandler<Query, Response>
{
    private readonly IPersistenceContext _context;

    public GetSubjectCourseByOrganizationNameHandler(IPersistenceContext context)
    {
        _context = context;
    }

    public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseQuery.Build(x => x
            .WithOrganizationName(request.OrganizationName)
            .WithLimit(2));

        GithubSubjectCourse? subjectCourse = await _context.SubjectCourses
            .QueryAsync(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        if (subjectCourse is null)
            throw EntityNotFoundException.SubjectCourse(request.OrganizationName).TaggedWithNotFound();

        GithubSubjectCourseDto dto = subjectCourse.ToDto();

        return new Response(dto);
    }
}