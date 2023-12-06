using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Notifications;

internal static class SubmissionDataCollectionFinished
{
    public record Notification(long TaskId) : INotification;
}