using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps;

public class SubjectCourseDumpExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    public StudentAssignmentKey? Key { get; set; }

    public record struct StudentAssignmentKey(Guid StudentId, Guid AssignmentId);
}