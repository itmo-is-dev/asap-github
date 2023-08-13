using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Domain.Users;

namespace Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;

public interface IGithubUserRepository
{
    IAsyncEnumerable<GithubUser> QueryAsync(GithubUserQuery query, CancellationToken cancellationToken);

    void AddRange(IReadOnlyCollection<GithubUser> users);

    void Add(GithubUser user);

    void Update(GithubUser user);
}