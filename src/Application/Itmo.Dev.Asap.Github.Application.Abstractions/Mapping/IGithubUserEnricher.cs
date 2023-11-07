using Itmo.Dev.Asap.Github.Application.Models.Users;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Mapping;

public interface IGithubUserEnricher
{
    Task<EnrichedGithubUser> MapAsync(GithubUser user, CancellationToken cancellationToken);
}