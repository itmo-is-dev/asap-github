using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;

public interface IProvisionedSubjectCourseRepository
{
    IAsyncEnumerable<ProvisionedSubjectCourse> QueryAsync(
        ProvisionedSubjectCourseQuery query,
        CancellationToken cancellationToken);

    void Add(ProvisionedSubjectCourse subjectCourse);

    void RemoveRange(IEnumerable<string> correlationIds);
}