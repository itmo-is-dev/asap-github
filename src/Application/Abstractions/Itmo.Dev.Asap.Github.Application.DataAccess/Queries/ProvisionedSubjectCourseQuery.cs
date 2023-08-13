using Itmo.Dev.Asap.Github.Application.DataAccess.Models;
using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.DataAccess.Queries;

[GenerateBuilder]
public partial record ProvisionedSubjectCourseQuery(
    string[] CorrelationIds,
    DateTime? Cursor,
    OrderDirection? OrderDirection,
    int? PageSize);