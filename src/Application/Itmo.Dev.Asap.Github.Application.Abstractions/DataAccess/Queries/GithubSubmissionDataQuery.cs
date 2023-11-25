using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;

[GenerateBuilder]
public partial record GithubSubmissionDataQuery(
    long TaskId,
    int PageSize,
    GithubSubmissionDataQuery.PageTokenModel? PageToken)
{
    public record struct PageTokenModel(Guid UserId, Guid AssignmentId);
}