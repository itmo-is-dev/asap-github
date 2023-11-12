using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;

public interface IGithubSubmissionRepository
{
    IAsyncEnumerable<GithubSubmission> QueryAsync(GithubSubmissionQuery query, CancellationToken cancellationToken);

    void Add(GithubSubmission submission);
}