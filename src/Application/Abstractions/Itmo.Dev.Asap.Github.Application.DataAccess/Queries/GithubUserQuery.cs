using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.DataAccess.Queries;

[GenerateBuilder]
public partial record GithubUserQuery(
    Guid[] Ids,
    long[] GithubUserIds,
    int? Limit);