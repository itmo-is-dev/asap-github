using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Notifications;

internal static class SubjectCourseMentorTeamUpdated
{
    public record Notification(Guid SubjectCourseId) : INotification;
}