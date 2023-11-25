using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;

public interface IGithubSubmissionLocatorService
{
    Task<string?> FindSubmissionCommitHash(
        GithubOrganizationModel organization,
        GithubRepositoryModel repository,
        string branchName,
        IReadOnlyCollection<long> mentors,
        CancellationToken cancellationToken);
}