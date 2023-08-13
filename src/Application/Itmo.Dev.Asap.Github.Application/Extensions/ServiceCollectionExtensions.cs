using Itmo.Dev.Asap.Github.Application.BackgroundServices;
using Itmo.Dev.Asap.Github.Application.Options;
using Itmo.Dev.Asap.Github.Application.Time;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Github.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        collection.AddSingleton<IDateTimeProvider, UtcNowDateTimeProvider>();

        collection
            .AddOptions<GithubInviteBackgroundServiceConfiguration>()
            .BindConfiguration("Application:Invites:Delay");

        collection.AddHostedService<GithubInviteBackgroundService>();

        collection
            .AddOptions<StaleProvisionedSubjectCourseEraserOptions>()
            .BindConfiguration("Application:Provisioning:SubjectCourses:Eraser");

        collection.AddHostedService<StaleProvisionedSubjectCourseEraser>();

        return collection;
    }
}