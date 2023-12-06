using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;

public interface IGithubSubmissionRepository
{
    IAsyncEnumerable<GithubSubmission> QueryAsync(GithubSubmissionQuery query, CancellationToken cancellationToken);

    IAsyncEnumerable<GithubSubmissionData> QueryDataAsync(
        GithubSubmissionDataQuery query,
        CancellationToken cancellationToken);

    void Add(GithubSubmission submission);

    void AddData(IReadOnlyCollection<GithubSubmissionData> data);

    void UpdateCommitHash(Guid submissionId, string commitHash);
}