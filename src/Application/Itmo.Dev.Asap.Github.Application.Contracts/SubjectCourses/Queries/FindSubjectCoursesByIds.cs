using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries;

public static class FindSubjectCoursesByIds
{
    public record Query(IEnumerable<Guid> SubjectCourseIds) : IRequest<Response>;

#pragma warning disable CA1724
    public record Response(IReadOnlyCollection<EnrichedGithubSubjectCourse> SubjectCourses);
}