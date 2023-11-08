namespace Itmo.Dev.Asap.Github.Application.Models.Submissions;

public record GithubSubmission(
    Guid Id,
    Guid AssignmentId,
    Guid UserId,
    DateTimeOffset CreatedAt,
    long OrganizationId,
    long RepositoryId,
    long PullRequestId);