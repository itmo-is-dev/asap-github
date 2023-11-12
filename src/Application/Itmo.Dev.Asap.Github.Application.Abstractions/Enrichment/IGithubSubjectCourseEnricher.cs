using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Enrichment;

public interface IGithubSubjectCourseEnricher
{
    Task<EnrichedGithubSubjectCourse> EnrichAsync(
        GithubSubjectCourse subjectCourse,
        CancellationToken cancellationToken);
}