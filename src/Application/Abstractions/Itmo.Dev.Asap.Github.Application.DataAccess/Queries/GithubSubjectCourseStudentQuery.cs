using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.DataAccess.Queries;

[GenerateBuilder]
public partial record GithubSubjectCourseStudentQuery(
    Guid[] SubjectCourseIds,
    Guid[] UserIds,
    long[] RepositoryIds);