using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Models.Assignments;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;

public interface IGithubAssignmentRepository
{
    IAsyncEnumerable<GithubAssignment> QueryAsync(GithubAssignmentQuery query, CancellationToken cancellationToken);

    void Add(GithubAssignment assignment);

    void Update(GithubAssignment assignment);
}