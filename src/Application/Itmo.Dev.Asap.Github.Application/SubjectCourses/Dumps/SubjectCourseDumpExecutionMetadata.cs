using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps;

public class SubjectCourseDumpExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    public string? SubmissionPageToken { get; set; }
}