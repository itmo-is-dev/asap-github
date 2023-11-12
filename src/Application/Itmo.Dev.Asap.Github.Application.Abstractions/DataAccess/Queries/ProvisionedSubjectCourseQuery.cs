using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Models;
using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;

[GenerateBuilder]
public partial record ProvisionedSubjectCourseQuery(
    string[] CorrelationIds,
    DateTimeOffset? Cursor,
    OrderDirection? OrderDirection,
    int? PageSize);