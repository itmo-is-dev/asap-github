using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;

[GenerateBuilder]
public partial record GithubAssignmentQuery(
    Guid[] Ids,
    Guid[] SubjectCourseIds,
    string[] BranchNames,
    long[] SubjectCourseOrganizationIds);