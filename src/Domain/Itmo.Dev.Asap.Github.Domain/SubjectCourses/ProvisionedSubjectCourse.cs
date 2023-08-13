namespace Itmo.Dev.Asap.Github.Domain.SubjectCourses;

public record ProvisionedSubjectCourse(
    string CorrelationId,
    string OrganizationName,
    string TemplateRepositoryName,
    string MentorTeamName,
    DateTime CreatedAt);