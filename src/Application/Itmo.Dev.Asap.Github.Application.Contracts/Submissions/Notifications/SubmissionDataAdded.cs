using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Notifications;

internal static class SubmissionDataAdded
{
    public record Notification(IReadOnlyCollection<GithubSubmissionData> Data) : INotification;
}