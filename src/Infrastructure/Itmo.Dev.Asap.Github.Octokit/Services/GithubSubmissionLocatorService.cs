using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Clients;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

internal class GithubSubmissionLocatorService : IGithubSubmissionLocatorService
{
    private readonly IGithubClientProvider _clientProvider;
    private readonly ILogger<GithubSubmissionLocatorService> _logger;

    public GithubSubmissionLocatorService(
        IGithubClientProvider clientProvider,
        ILogger<GithubSubmissionLocatorService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<string?> FindSubmissionCommitHash(
        GithubOrganizationModel organization,
        string repository,
        string branchName,
        CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetOrganizationClientAsync(organization.Id, cancellationToken);

        _logger.LogInformation(
            "[{Organization}/{Repository} - {Branch}] started hash fetching",
            organization.Name,
            repository,
            branchName);

        var pullRequestRequest = new PullRequestRequest
        {
            Head = branchName,
            SortDirection = SortDirection.Descending,
            SortProperty = PullRequestSort.Created,
            State = ItemStateFilter.All,
        };

        var apiOptions = new ApiOptions { PageSize = 1 };

        IReadOnlyList<PullRequest> pullRequests = await client.Repository.PullRequest
            .GetAllForRepository(organization.Name, repository, pullRequestRequest, apiOptions);

        if (pullRequests is [])
        {
            _logger.LogWarning(
                "[{Organization}/{Repository} - {Branch}] failed to find pull request",
                organization.Name,
                repository,
                branchName);

            return null;
        }

        int pullRequestNumber = pullRequests[0].Number;

        IReadOnlyList<PullRequestReviewComment> reviewComments = await client.Repository.PullRequest.ReviewComment
            .GetAll(organization.Name, repository, pullRequestNumber);

        PullRequestReviewComment? firstReview = reviewComments.MinBy(x => x.CreatedAt);

        if (firstReview is null)
        {
            _logger.LogWarning(
                "[{Organization}/{Repository} - {Branch}] failed to find review comment",
                organization.Name,
                repository,
                branchName);

            return null;
        }

        IReadOnlyList<PullRequestCommit> commits = await client.Repository.PullRequest
            .Commits(organization.Name, repository, pullRequestNumber);

        PullRequestCommit? commit = commits
            .Where(x => x.Commit.Author.Date <= firstReview.CreatedAt)
            .MaxBy(x => x.Commit.Author.Date);

        if (commit is null)
        {
            _logger.LogWarning(
                "[{Organization}/{Repository} - {Branch}] failed to find commit hash",
                organization.Name,
                repository,
                branchName);
        }

        return commit?.Sha;
    }
}