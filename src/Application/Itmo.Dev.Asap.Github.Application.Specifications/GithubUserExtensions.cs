using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Common.Exceptions.Entities;
using Itmo.Dev.Asap.Github.Common.Extensions;
using Itmo.Dev.Asap.Github.Domain.Users;

namespace Itmo.Dev.Asap.Github.Application.Specifications;

public static class GithubUserExtensions
{
    public static async Task<GithubUser> GetForGithubIdAsync(
        this IGithubUserRepository repository,
        long githubUserId,
        CancellationToken cancellationToken = default)
    {
        var query = GithubUserQuery.Build(x => x
            .WithGithubUserId(githubUserId)
            .WithLimit(1));

        GithubUser? user = await repository
            .QueryAsync(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        return user ?? throw EntityNotFoundException.User().TaggedWithNotFound();
    }
}