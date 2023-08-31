namespace Itmo.Dev.Asap.Github.Tests.Models;

public record GithubSubjectCourseModel(
    Guid Id,
    long OrganizationId,
    long TemplateRepositoryId,
    long MentorTeamId);