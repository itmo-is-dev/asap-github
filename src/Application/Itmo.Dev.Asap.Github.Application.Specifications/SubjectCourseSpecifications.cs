using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Common.Exceptions.Entities;
using Itmo.Dev.Asap.Github.Common.Extensions;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Application.Specifications;

public static class SubjectCourseSpecifications
{
    public static IAsyncEnumerable<GithubSubjectCourse> ForOrganizationName(
        this IGithubSubjectCourseRepository repository,
        string organizationName,
        CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseQuery.Build(x => x.WithOrganizationName(organizationName));
        return repository.QueryAsync(query, cancellationToken);
    }

    public static async Task<GithubSubjectCourse> GetByIdAsync(
        this IGithubSubjectCourseRepository repository,
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseQuery.Build(x => x.WithId(id).WithLimit(1));

        GithubSubjectCourse? subjectCourse = await repository
            .QueryAsync(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        return subjectCourse ?? throw EntityNotFoundException.SubjectCourse().TaggedWithNotFound();
    }
}