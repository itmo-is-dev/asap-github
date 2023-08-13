using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.DataAccess.Queries;

[GenerateBuilder]
public partial record GithubUserQuery(IReadOnlyCollection<Guid> Ids, IReadOnlyCollection<string> Usernames, int? Limit);