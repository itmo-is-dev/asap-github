using Itmo.Dev.Asap.Github.Application.Models.Users;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Enrichment;

public interface IGithubUserEnricher
{
    Task<EnrichedGithubUser> EnrichAsync(GithubUser user, CancellationToken cancellationToken);
}