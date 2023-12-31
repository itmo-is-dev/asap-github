using Itmo.Dev.Asap.Github.Application.Abstractions.Enrichment;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Parsers;
using Itmo.Dev.Asap.Github.Application.Enrichment;
using Itmo.Dev.Asap.Github.Application.Invites;
using Itmo.Dev.Asap.Github.Application.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps;
using Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Services;
using Itmo.Dev.Asap.Github.Application.SubjectCourses.Options;
using Itmo.Dev.Asap.Github.Application.Submissions.Commands;
using Itmo.Dev.Platform.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Itmo.Dev.Asap.Github.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        collection
            .AddOptions<GithubInviteBackgroundServiceConfiguration>()
            .BindConfiguration("Application:Invites:Delay");

        collection.AddHostedService<GithubInviteBackgroundService>();

        collection
            .AddOptions<StaleProvisionedSubjectCourseEraserOptions>()
            .BindConfiguration("Application:Provisioning:SubjectCourses:Eraser");

        collection
            .AddOptions<SubjectCourseOrganizationUpdateOptions>()
            .BindConfiguration("Application:SubjectCourseOrganizationUpdate");

        collection
            .AddOptions<SubjectCourseDumpOptions>()
            .BindConfiguration("Application:SubjectCourseDump");

        collection.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<IAssemblyMarker>());

        collection.AddHostedService<StaleProvisionedSubjectCourseEraser>();

        collection.AddScoped<IGithubUserEnricher, GithubUserEnricher>();
        collection.AddScoped<IGithubSubjectCourseEnricher, GithubSubjectCourseEnricher>();

        collection.TryAddSingleton<ISubmissionCommandParser, SubmissionCommandParser>();

        collection.AddSubjectCourseContentDump();

        collection.AddUtcDateTimeProvider();

        return collection;
    }

    private static void AddSubjectCourseContentDump(this IServiceCollection collection)
    {
        collection.AddScoped<StudentRepositoryProvider>();
        collection.AddScoped<SubmissionDataFactory>();
        collection.AddScoped<SubmissionDumpPageHandler>();
        collection.AddScoped<SubmissionHashProvider>();
    }
}