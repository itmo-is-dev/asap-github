using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Models.Users;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;

public interface IGithubUserRepository
{
    IAsyncEnumerable<GithubUser> QueryAsync(GithubUserQuery query, CancellationToken cancellationToken);

    void AddRange(IReadOnlyCollection<GithubUser> users);

    void Add(GithubUser user);

    void Update(GithubUser user);
}