using Itmo.Dev.Asap.Github.Application.Dto.SubjectCourses;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries;

public static class GetSubjectCourseByOrganizationName
{
    public record Query(string OrganizationName) : IRequest<Response>;

    public record Response(GithubSubjectCourseDto SubjectCourse);
}