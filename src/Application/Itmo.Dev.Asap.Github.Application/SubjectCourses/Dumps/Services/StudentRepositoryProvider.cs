using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Models;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Services;

public class StudentRepositoryProvider
{
    private readonly IPersistenceContext _context;
    private readonly IGithubRepositoryService _repositoryService;

    public StudentRepositoryProvider(IPersistenceContext context, IGithubRepositoryService repositoryService)
    {
        _context = context;
        _repositoryService = repositoryService;
    }

    public async IAsyncEnumerable<StudentRepositoryModel> GetStudentRepositories(
        GithubSubjectCourse subjectCourse,
        IEnumerable<Guid> userIds,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseStudentQuery.Build(x => x
            .WithSubjectCourseId(subjectCourse.Id)
            .WithUserIds(userIds));

        IAsyncEnumerable<GithubSubjectCourseStudent> students = _context.SubjectCourses
            .QueryStudentsAsync(query, cancellationToken);

        await foreach (GithubSubjectCourseStudent student in students)
        {
            GithubRepositoryModel? repository = await _repositoryService.FindByIdAsync(
                subjectCourse.OrganizationId,
                student.RepositoryId,
                cancellationToken);

            if (repository is not null)
                yield return new StudentRepositoryModel(student.User.Id, repository);
        }
    }
}