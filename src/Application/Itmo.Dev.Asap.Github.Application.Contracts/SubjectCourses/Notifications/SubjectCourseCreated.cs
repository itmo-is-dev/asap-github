using Itmo.Dev.Asap.Github.Application.Dto.SubjectCourses;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Notifications;

internal static class SubjectCourseCreated
{
    public record Notification(string? CorrelationId, SubjectCourseDto SubjectCourse) : INotification;
}