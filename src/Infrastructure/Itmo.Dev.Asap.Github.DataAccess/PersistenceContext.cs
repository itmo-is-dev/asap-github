using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;
using Itmo.Dev.Platform.Postgres.UnitOfWork;
using System.Data;

namespace Itmo.Dev.Asap.Github.DataAccess;

internal class PersistenceContext : IPersistenceContext
{
    private readonly IUnitOfWork _unitOfWork;

    public PersistenceContext(
        IUnitOfWork unitOfWork,
        IGithubAssignmentRepository assignments,
        IGithubSubmissionRepository submissions,
        IGithubUserRepository users,
        IGithubSubjectCourseRepository subjectCourses,
        IProvisionedSubjectCourseRepository provisionedSubjectCourses)
    {
        _unitOfWork = unitOfWork;
        Assignments = assignments;
        Submissions = submissions;
        Users = users;
        SubjectCourses = subjectCourses;
        ProvisionedSubjectCourses = provisionedSubjectCourses;
    }

    public IGithubAssignmentRepository Assignments { get; }

    public IGithubSubmissionRepository Submissions { get; }

    public IGithubUserRepository Users { get; }

    public IGithubSubjectCourseRepository SubjectCourses { get; }

    public IProvisionedSubjectCourseRepository ProvisionedSubjectCourses { get; }

    public ValueTask CommitAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken)
    {
        return _unitOfWork.CommitAsync(isolationLevel, cancellationToken);
    }

    public ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        return _unitOfWork.CommitAsync(IsolationLevel.ReadCommitted, cancellationToken);
    }
}