using Itmo.Dev.Asap.Github.Application.Dto.SubjectCourses;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;

namespace Itmo.Dev.Asap.Application.Abstractions.Mapping;

public interface IGithubSubjectCourseMapper
{
    Task<GithubSubjectCourseDto> MapAsync(GithubSubjectCourse subjectCourse, CancellationToken cancellationToken);
}