using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;

public interface IProvisionedSubjectCourseRepository
{
    IAsyncEnumerable<ProvisionedSubjectCourse> QueryAsync(
        ProvisionedSubjectCourseQuery query,
        CancellationToken cancellationToken);

    void Add(ProvisionedSubjectCourse subjectCourse);

    void RemoveRange(IEnumerable<string> correlationIds);
}