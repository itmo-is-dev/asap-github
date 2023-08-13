using Itmo.Dev.Asap.Github.Application.Contracts.Assignments.Notifications;
using Itmo.Dev.Asap.Github.Presentation.Kafka.Mapping;
using Itmo.Dev.Asap.Kafka;
using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.Kafka.Extensions;
using MediatR;

namespace Itmo.Dev.Asap.Github.Presentation.Kafka.ConsumerHandlers;

public class AssignmentCreatedHandler : IKafkaMessageHandler<AssignmentCreatedKey, AssignmentCreatedValue>
{
    private readonly IMediator _mediator;

    public AssignmentCreatedHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async ValueTask HandleAsync(
        IEnumerable<ConsumerKafkaMessage<AssignmentCreatedKey, AssignmentCreatedValue>> messages,
        CancellationToken cancellationToken)
    {
        IEnumerable<AssignmentCreated.Notification> notifications = messages
            .GetLatestByKey()
            .Select(x => new AssignmentCreated.Notification(x.Value.MapTo()));

        foreach (AssignmentCreated.Notification notification in notifications)
        {
            await _mediator.Publish(notification, cancellationToken);
        }
    }
}