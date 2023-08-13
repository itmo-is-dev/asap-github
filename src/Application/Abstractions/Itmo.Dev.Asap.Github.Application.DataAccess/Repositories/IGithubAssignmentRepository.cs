using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Domain.Assignments;

namespace Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;

public interface IGithubAssignmentRepository
{
    IAsyncEnumerable<GithubAssignment> QueryAsync(GithubAssignmentQuery query, CancellationToken cancellationToken);

    void Add(GithubAssignment assignment);

    void Update(GithubAssignment assignment);
}