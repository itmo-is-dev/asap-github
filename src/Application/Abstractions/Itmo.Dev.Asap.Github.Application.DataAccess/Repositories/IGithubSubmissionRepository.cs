using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Domain.Submissions;

namespace Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;

public interface IGithubSubmissionRepository
{
    IAsyncEnumerable<GithubSubmission> QueryAsync(GithubSubmissionQuery query, CancellationToken cancellationToken);

    void Add(GithubSubmission submission);
}