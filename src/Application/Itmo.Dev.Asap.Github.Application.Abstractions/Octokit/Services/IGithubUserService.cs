using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;

public interface IGithubUserService
{
    Task<GithubUserModel?> FindByIdAsync(long userId, CancellationToken cancellationToken);

    Task<GithubUserModel?> FindByUsernameAsync(string username, CancellationToken cancellationToken);
}