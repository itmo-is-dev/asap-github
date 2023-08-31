namespace Itmo.Dev.Asap.Github.Domain.SubjectCourses;

public record ProvisionedSubjectCourse(
    string CorrelationId,
    long OrganizationId,
    long TemplateRepositoryId,
    long MentorTeamId,
    DateTime CreatedAt);