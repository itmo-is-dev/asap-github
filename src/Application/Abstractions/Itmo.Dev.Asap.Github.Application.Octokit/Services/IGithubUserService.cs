using Itmo.Dev.Asap.Github.Application.Octokit.Models;

namespace Itmo.Dev.Asap.Github.Application.Octokit.Services;

public interface IGithubUserService
{
    Task<GithubUserModel?> FindByIdAsync(long userId, CancellationToken cancellationToken);
}