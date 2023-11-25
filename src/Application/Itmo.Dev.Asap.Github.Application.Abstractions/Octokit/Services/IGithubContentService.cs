using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Results;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;

public interface IGithubContentService
{
    Task<GetRepositoryContentResult> GetRepositoryContentAsync(
        long organizationId,
        long repositoryId,
        string hash,
        CancellationToken cancellationToken);
}