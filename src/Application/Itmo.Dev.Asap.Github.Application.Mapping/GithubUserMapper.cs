using Itmo.Dev.Asap.Application.Abstractions.Mapping;
using Itmo.Dev.Asap.Github.Application.Dto.Users;
using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Domain.Users;

namespace Itmo.Dev.Asap.Github.Application.Mapping;

internal class GithubUserMapper : IGithubUserMapper
{
    private readonly IGithubUserService _service;

    public GithubUserMapper(IGithubUserService service)
    {
        _service = service;
    }

    public async Task<GithubUserDto> MapAsync(GithubUser user, CancellationToken cancellationToken)
    {
        GithubUserModel? githubUser = await _service.FindByIdAsync(user.GithubId, cancellationToken);
        return new GithubUserDto(user.Id, user.GithubId, githubUser?.Username ?? string.Empty);
    }
}