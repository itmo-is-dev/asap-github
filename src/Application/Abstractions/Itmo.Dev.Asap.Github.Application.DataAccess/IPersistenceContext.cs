using Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;
using System.Data;

namespace Itmo.Dev.Asap.Github.Application.DataAccess;

public interface IPersistenceContext
{
    IGithubAssignmentRepository Assignments { get; }

    IGithubSubmissionRepository Submissions { get; }

    IGithubUserRepository Users { get; }

    IGithubSubjectCourseRepository SubjectCourses { get; }

    IProvisionedSubjectCourseRepository ProvisionedSubjectCourses { get; }

    ValueTask CommitAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken);

    ValueTask CommitAsync(CancellationToken cancellationToken);
}