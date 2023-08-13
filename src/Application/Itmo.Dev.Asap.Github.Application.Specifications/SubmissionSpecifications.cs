using Itmo.Dev.Asap.Github.Application.DataAccess.Models;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Application.Dto.PullRequests;
using Itmo.Dev.Asap.Github.Common.Exceptions.Entities;
using Itmo.Dev.Asap.Github.Common.Extensions;
using Itmo.Dev.Asap.Github.Domain.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Specifications;

public static class SubmissionSpecifications
{
    public static async Task<GithubSubmission> GetSubmissionForPullRequestAsync(
        this IGithubSubmissionRepository repository,
        PullRequestDto pullRequest,
        CancellationToken cancellationToken = default)
    {
        var query = GithubSubmissionQuery.Build(x => x
            .WithRepositoryName(pullRequest.Repository)
            .WithPullRequestNumber(pullRequest.PullRequestNumber)
            .WithOrganizationName(pullRequest.Organization)
            .WithAssignmentBranchName(pullRequest.BranchName)
            .WithOrderByCreatedAt(OrderDirection.Descending));

        GithubSubmission? submission = await repository
            .QueryAsync(query, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        return submission ?? throw EntityNotFoundException.Submission().TaggedWithNotFound();
    }
}