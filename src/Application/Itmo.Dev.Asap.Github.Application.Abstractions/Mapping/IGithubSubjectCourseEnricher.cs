using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Mapping;

public interface IGithubSubjectCourseEnricher
{
    Task<EnrichedGithubSubjectCourse> MapAsync(GithubSubjectCourse subjectCourse, CancellationToken cancellationToken);
}