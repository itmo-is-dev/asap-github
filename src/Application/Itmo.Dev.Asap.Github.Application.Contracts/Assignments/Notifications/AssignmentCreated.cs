using Itmo.Dev.Asap.Github.Application.Models.Assignments;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Assignments.Notifications;

internal static class AssignmentCreated
{
    public record Notification(GithubAssignment Assignment) : INotification;
}