using Itmo.Dev.Asap.Github.Application.Dto.Assignments;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Assignments.Notifications;

internal static class AssignmentCreated
{
    public record Notification(AssignmentDto Assignment) : INotification;
}