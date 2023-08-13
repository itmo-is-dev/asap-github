using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.DataAccess.Queries;

[GenerateBuilder]
public partial record GithubSubjectCourseQuery(
    IReadOnlyCollection<Guid> Ids,
    IReadOnlyCollection<string> OrganizationNames,
    IReadOnlyCollection<string> TemplateRepositoryNames,
    IReadOnlyCollection<string> MentorTeamNames,
    int? Limit);