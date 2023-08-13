using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.DataAccess.Queries;

[GenerateBuilder]
public partial record GithubAssignmentQuery(
    IReadOnlyCollection<Guid> Ids,
    IReadOnlyCollection<Guid> SubjectCourseIds,
    IReadOnlyCollection<string> BranchNames,
    IReadOnlyCollection<string> SubjectCourseOrganizationNames);