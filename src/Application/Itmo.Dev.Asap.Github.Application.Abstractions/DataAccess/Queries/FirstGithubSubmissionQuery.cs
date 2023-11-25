using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;

[GenerateBuilder]
public partial record FirstGithubSubmissionQuery(
    Guid? UserId,
    Guid? AssignmentId,
    int PageSize);