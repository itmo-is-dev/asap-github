using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps;

public record SubjectCourseDumpMetadata(Guid SubjectCourseId) : IBackgroundTaskMetadata;