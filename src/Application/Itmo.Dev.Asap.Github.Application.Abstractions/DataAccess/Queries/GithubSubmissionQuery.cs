using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Models;
using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;

[GenerateBuilder]
public partial record GithubSubmissionQuery(
    Guid[] Ids,
    long[] RepositoryIds,
    long[] PullRequestIds,
    long[] OrganizationIds,
    string[] AssignmentBranchNames,
    OrderDirection? OrderByCreatedAt)
{
    public bool HasOrderParameters => OrderByCreatedAt is not null;
}