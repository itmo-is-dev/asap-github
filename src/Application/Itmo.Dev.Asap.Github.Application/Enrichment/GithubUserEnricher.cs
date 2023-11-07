using Itmo.Dev.Asap.Github.Application.Abstractions.Mapping;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Models.Users;

namespace Itmo.Dev.Asap.Github.Application.Enrichment;

internal class GithubUserEnricher : IGithubUserEnricher
{
    private readonly IGithubUserService _service;

    public GithubUserEnricher(IGithubUserService service)
    {
        _service = service;
    }

    public async Task<EnrichedGithubUser> MapAsync(GithubUser user, CancellationToken cancellationToken)
    {
        GithubUserModel? githubUser = await _service.FindByIdAsync(user.GithubId, cancellationToken);
        return new EnrichedGithubUser(user.Id, user.GithubId, githubUser?.Username ?? string.Empty);
    }
}