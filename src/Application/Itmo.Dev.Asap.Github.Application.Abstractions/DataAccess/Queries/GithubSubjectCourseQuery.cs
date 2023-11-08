using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;

[GenerateBuilder]
public partial record GithubSubjectCourseQuery(
    Guid[] Ids,
    long[] OrganizationIds,
    long[] TemplateRepositoryIds,
    long[] MentorTeamIds,
    int? Limit);