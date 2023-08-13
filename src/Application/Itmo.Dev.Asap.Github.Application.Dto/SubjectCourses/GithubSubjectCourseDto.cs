namespace Itmo.Dev.Asap.Github.Application.Dto.SubjectCourses;

public record GithubSubjectCourseDto(
    Guid Id,
    string OrganizationName,
    string TemplateRepositoryName,
    string MentorTeamName);