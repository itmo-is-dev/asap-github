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
        GithubRepositoryModel repository,
        string branchName,
        IReadOnlyCollection<long> mentors,
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

        var apiOptions = new ApiOptions
        {
            PageSize = 1,
            StartPage = 1,
        };

        IReadOnlyList<PullRequest> pullRequests;

        do
        {
            pullRequests = await client.Repository.PullRequest
                .GetAllForRepository(organization.Name, repository.Name, pullRequestRequest, apiOptions);

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

            string? sha = await FindCommitAsync(
                organization,
                repository,
                branchName,
                mentors,
                client,
                pullRequestNumber);

            if (sha is not null)
                return sha;

            apiOptions.StartPage++;
        }
        while (pullRequests is not []);

        return null;
    }

    private async Task<string?> FindCommitAsync(
        GithubOrganizationModel organization,
        GithubRepositoryModel repository,
        string branchName,
        IReadOnlyCollection<long> mentors,
        IGitHubClient client,
        int pullRequestNumber)
    {
        IReadOnlyList<PullRequestReview> reviewComments = await client.PullRequest.Review
            .GetAll(repository.Id, pullRequestNumber);

        PullRequestReview? review = reviewComments.FirstOrDefault(x => mentors.Contains(x.User.Id));

        if (review is null)
        {
            _logger.LogWarning(
                "[{Organization}/{Repository} - {Branch}] failed to find review comment",
                organization.Name,
                repository,
                branchName);

            return null;
        }

        IReadOnlyList<PullRequestCommit> commits = await client.Repository.PullRequest
            .Commits(organization.Name, repository.Name, pullRequestNumber);

        PullRequestCommit? commit = commits
            .Where(x => x.Commit.Author.Date <= review.SubmittedAt)
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