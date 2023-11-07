namespace Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;

public record ProvisionedSubjectCourse(
    string CorrelationId,
    long OrganizationId,
    long TemplateRepositoryId,
    long MentorTeamId,
    DateTimeOffset CreatedAt);