using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps;

public record SubjectCourseDumpError(string Message) : IBackgroundTaskError;