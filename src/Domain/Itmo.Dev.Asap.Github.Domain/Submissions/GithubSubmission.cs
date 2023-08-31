using RichEntity.Annotations;

namespace Itmo.Dev.Asap.Github.Domain.Submissions;

public partial class GithubSubmission : IEntity<Guid>
{
    public GithubSubmission(
        Guid id,
        Guid assignmentId,
        Guid userId,
        DateTime createdAt,
        long organizationId,
        long repositoryId,
        long pullRequestId) : this(id)
    {
        AssignmentId = assignmentId;
        UserId = userId;
        CreatedAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
        RepositoryId = repositoryId;
        PullRequestId = pullRequestId;
        OrganizationId = organizationId;
    }

    public Guid AssignmentId { get; protected init; }

    public Guid UserId { get; protected init; }

    public DateTime CreatedAt { get; protected init; }

    public long OrganizationId { get; protected set; }

    public long RepositoryId { get; protected set; }

    public long PullRequestId { get; protected set; }
}