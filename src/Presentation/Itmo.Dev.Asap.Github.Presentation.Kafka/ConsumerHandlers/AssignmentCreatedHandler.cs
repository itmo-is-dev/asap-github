using Itmo.Dev.Asap.Github.Application.Contracts.Assignments.Notifications;
using Itmo.Dev.Asap.Github.Application.Models.Assignments;
using Itmo.Dev.Asap.Github.Common.Tools;
using Itmo.Dev.Asap.Kafka;
using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Extensions;
using MediatR;

namespace Itmo.Dev.Asap.Github.Presentation.Kafka.ConsumerHandlers;

public class AssignmentCreatedHandler : IKafkaConsumerHandler<AssignmentCreatedKey, AssignmentCreatedValue>
{
    private readonly IMediator _mediator;

    public AssignmentCreatedHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async ValueTask HandleAsync(
        IEnumerable<IKafkaConsumerMessage<AssignmentCreatedKey, AssignmentCreatedValue>> messages,
        CancellationToken cancellationToken)
    {
        IEnumerable<AssignmentCreated.Notification> notifications = messages
            .GetLatestByKey()
            .Select(x => new AssignmentCreated.Notification(Map(x.Value)));

        foreach (AssignmentCreated.Notification notification in notifications)
        {
            await _mediator.Publish(notification, cancellationToken);
        }
    }

    private static GithubAssignment Map(AssignmentCreatedValue value)
    {
        return new GithubAssignment(
            value.Id.ToGuid(),
            value.SubjectCourseId.ToGuid(),
            value.ShortName,
            RepositoryPath: string.Empty);
    }
}