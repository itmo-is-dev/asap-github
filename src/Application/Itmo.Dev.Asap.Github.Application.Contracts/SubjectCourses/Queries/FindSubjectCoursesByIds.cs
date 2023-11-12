using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries;

public static class FindSubjectCoursesByIds
{
    public record Query(IEnumerable<Guid> SubjectCourseIds) : IRequest<Response>;

    public record Response(IReadOnlyCollection<EnrichedGithubSubjectCourse> SubjectCourses);
}