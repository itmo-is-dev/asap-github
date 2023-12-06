using Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps;
using Itmo.Dev.Platform.BackgroundTasks.Configuration.Builders;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Asap.Github.Application.Extensions;

public static class BackgroundTaskConfigurationExtensions
{
    public static IBackgroundTaskConfigurationBuilder AddApplicationBackgroundTasks(
        this IBackgroundTaskConfigurationBuilder builder)
    {
        return builder
            .AddBackgroundTask(x => x
                .WithMetadata<SubjectCourseDumpMetadata>()
                .WithExecutionMetadata<SubjectCourseDumpExecutionMetadata>()
                .WithResult<EmptyExecutionResult>()
                .WithError<SubjectCourseDumpError>()
                .HandleBy<SubjectCourseDumpTask>());
    }
}