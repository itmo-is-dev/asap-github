using Itmo.Dev.Asap.Github.Domain.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Tests.Models;

public record GithubSubjectCourseModel(
    Guid Id,
    string OrganizationName,
    string TemplateRepositoryName,
    string MentorTeamName)
{
    public GithubSubjectCourse ToEntity()
    {
        return new GithubSubjectCourse(Id, OrganizationName, TemplateRepositoryName, MentorTeamName);
    }
}