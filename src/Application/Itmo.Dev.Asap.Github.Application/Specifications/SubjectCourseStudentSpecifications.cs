using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Application.Specifications;

public static class SubjectCourseStudentSpecifications
{
    public static async Task<GithubSubjectCourseStudent?> FindSubjectCourseStudentByRepositoryId(
        this IGithubSubjectCourseRepository repository,
        long repositoryId,
        CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseStudentQuery.Build(x => x.WithRepositoryId(repositoryId));
        return await repository.QueryStudentsAsync(query, cancellationToken).FirstOrDefaultAsync(cancellationToken);
    }
}