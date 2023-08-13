using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.DataAccess.Repositories;
using Itmo.Dev.Platform.Postgres.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Github.DataAccess.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection collection)
    {
        collection.AddPlatformPostgres(x => x
            .BindConfiguration("Infrastructure:DataAccess:PostgresConfiguration"));

        collection.AddScoped<IPersistenceContext, PersistenceContext>();

        collection.AddScoped<IGithubAssignmentRepository, GithubAssignmentRepository>();
        collection.AddScoped<IGithubSubjectCourseRepository, GithubSubjectCourseRepository>();
        collection.AddScoped<IGithubSubmissionRepository, GithubSubmissionRepository>();
        collection.AddScoped<IGithubUserRepository, GithubUserRepository>();
        collection.AddScoped<IProvisionedSubjectCourseRepository, ProvisionedSubjectCourseRepository>();

        collection.AddPlatformMigrations(typeof(IAssemblyMarker).Assembly);

        return collection;
    }

    public static Task UseDataAccessAsync(this IServiceScope scope, CancellationToken cancellationToken)
    {
        return scope.UsePlatformMigrationsAsync(cancellationToken);
    }
}