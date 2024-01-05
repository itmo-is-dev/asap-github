using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Application.Models.PullRequests;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Specifications;

public static class SubmissionSpecifications
{
    public static async Task<GithubSubmission?> FindSubmissionForPullRequestAsync(
        this IGithubSubmissionRepository repository,
        PullRequestDto pullRequest,
        CancellationToken cancellationToken = default)
    {
        var query = GithubSubmissionQuery.Build(x => x
            .WithRepositoryId(pullRequest.RepositoryId)
            .WithPullRequestId(pullRequest.PullRequestId)
            .WithOrganizationId(pullRequest.OrganizationId)
            .WithAssignmentBranchName(pullRequest.BranchName)
            .WithOrderByCreatedAt(OrderDirection.Descending));

        return await repository
            .QueryAsync(query, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);
    }
}