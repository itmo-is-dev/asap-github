using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;

public interface IGithubSubmissionLocatorService
{
    Task<string?> FindSubmissionCommitHash(
        GithubOrganizationModel organization,
        string repository,
        string branchName,
        CancellationToken cancellationToken);
}