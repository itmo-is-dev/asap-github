using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;

public interface IGithubSubjectCourseRepository
{
    IAsyncEnumerable<GithubSubjectCourse> QueryAsync(
        GithubSubjectCourseQuery query,
        CancellationToken cancellationToken);

    void Add(GithubSubjectCourse subjectCourse);

    void Update(GithubSubjectCourse subjectCourse);
}