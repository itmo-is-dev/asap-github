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
        var query = GithubUserQuery.Build(x => x.WithGithubUserId(githubUserId));

        GithubUser? user = await repository
            .QueryAsync(query, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        return user ?? throw EntityNotFoundException.User().TaggedWithNotFound();
    }

    public static async Task<GithubUser?> FindByIdAsync(
        this IGithubUserRepository repository,
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = GithubUserQuery.Build(x => x.WithId(id));
        return await repository.QueryAsync(query, cancellationToken).FirstOrDefaultAsync(cancellationToken);
    }
}