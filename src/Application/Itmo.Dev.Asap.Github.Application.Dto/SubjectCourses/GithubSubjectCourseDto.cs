namespace Itmo.Dev.Asap.Github.Application.Dto.SubjectCourses;

public record GithubSubjectCourseDto(
    Guid Id,
    long OrganizationId,
    string OrganizationName,
    long TemplateRepositoryId,
    string TemplateRepositoryName,
    long MentorTeamId,
    string MentorTeamName);