using Itmo.Dev.Asap.Github.Application.Dto.SubjectCourses;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Application.Mapping;

public static class SubjectCourseMapping
{
    public static GithubSubjectCourseDto ToDto(this GithubSubjectCourse subjectCourse)
    {
        return new GithubSubjectCourseDto(
            subjectCourse.Id,
            subjectCourse.OrganizationName,
            subjectCourse.TemplateRepositoryName,
            subjectCourse.MentorTeamName);
    }
}