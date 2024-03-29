using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Application.Specifications;

public static class SubjectCourseSpecifications
{
    public static IAsyncEnumerable<GithubSubjectCourse> ForOrganization(
        this IGithubSubjectCourseRepository repository,
        long organizationId,
        CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseQuery.Build(x => x.WithOrganizationId(organizationId));
        return repository.QueryAsync(query, cancellationToken);
    }

    public static async Task<GithubSubjectCourse?> FindByIdAsync(
        this IGithubSubjectCourseRepository repository,
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseQuery.Build(x => x.WithId(id).WithLimit(1));

        return await repository
            .QueryAsync(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public static async Task<GithubSubjectCourse?> GetByIdAsync(
        this IGithubSubjectCourseRepository repository,
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseQuery.Build(x => x.WithId(id).WithLimit(1));

        return await repository
            .QueryAsync(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);
    }
}