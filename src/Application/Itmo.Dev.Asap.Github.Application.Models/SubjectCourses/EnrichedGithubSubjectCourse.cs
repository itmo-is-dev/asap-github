namespace Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;

public record EnrichedGithubSubjectCourse(
    Guid Id,
    long OrganizationId,
    string OrganizationName,
    long TemplateRepositoryId,
    string TemplateRepositoryName,
    long MentorTeamId,
    string MentorTeamName);