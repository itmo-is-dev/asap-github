using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;

public interface IGithubSubjectCourseRepository
{
    IAsyncEnumerable<GithubSubjectCourse> QueryAsync(
        GithubSubjectCourseQuery query,
        CancellationToken cancellationToken);

    IAsyncEnumerable<GithubSubjectCourseStudent> QueryStudentsAsync(
        GithubSubjectCourseStudentQuery query,
        CancellationToken cancellationToken);

    void Add(GithubSubjectCourse subjectCourse);

    void AddStudent(GithubSubjectCourseStudent student);

    void Update(GithubSubjectCourse subjectCourse);
}