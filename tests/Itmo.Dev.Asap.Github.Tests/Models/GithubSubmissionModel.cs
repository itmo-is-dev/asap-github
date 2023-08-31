namespace Itmo.Dev.Asap.Github.Tests.Models;

public record GithubSubmissionModel(
    Guid Id,
    Guid AssignmentId,
    Guid UserId,
    DateTime CreatedAt,
    long OrganizationId,
    long RepositoryId,
    long PullRequestId);