using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;

[GenerateBuilder]
public partial record GithubSubjectCourseStudentQuery(
    Guid[] SubjectCourseIds,
    Guid[] UserIds,
    long[] RepositoryIds);