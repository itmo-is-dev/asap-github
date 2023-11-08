using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Notifications;

internal static class SubjectCourseCreated
{
    public sealed record SubjectCourse(Guid Id, string Title);

    public record Notification(string? CorrelationId, SubjectCourse SubjectCourse) : INotification;
}