using Itmo.Dev.Asap.Github.Domain.Submissions;

namespace Itmo.Dev.Asap.Github.Tests.Models;

public record GithubSubmissionModel(
    Guid Id,
    Guid AssignmentId,
    Guid UserId,
    DateTime CreatedAt,
    string Organization,
    string Repository,
    long PullRequestNumber)
{
    public GithubSubmission ToEntity()
    {
        return new GithubSubmission(Id, AssignmentId, UserId, CreatedAt, Organization, Repository, PullRequestNumber);
    }
}