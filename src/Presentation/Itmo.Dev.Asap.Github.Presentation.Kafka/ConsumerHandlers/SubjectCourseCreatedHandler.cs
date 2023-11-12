using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Notifications;
using Itmo.Dev.Asap.Github.Common.Tools;
using Itmo.Dev.Asap.Kafka;
using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.Kafka.Extensions;
using MediatR;

namespace Itmo.Dev.Asap.Github.Presentation.Kafka.ConsumerHandlers;

public class SubjectCourseCreatedHandler : IKafkaMessageHandler<SubjectCourseCreatedKey, SubjectCourseCreatedValue>
{
    private readonly IMediator _mediator;

    public SubjectCourseCreatedHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async ValueTask HandleAsync(
        IEnumerable<ConsumerKafkaMessage<SubjectCourseCreatedKey, SubjectCourseCreatedValue>> messages,
        CancellationToken cancellationToken)
    {
        IEnumerable<SubjectCourseCreated.Notification> notifications = messages
            .GetLatestByKey()
            .Select(x => Map(x.Value));

        foreach (SubjectCourseCreated.Notification notification in notifications)
        {
            await _mediator.Publish(notification, cancellationToken);
        }
    }

    private static SubjectCourseCreated.Notification Map(SubjectCourseCreatedValue value)
    {
        var subjectCourse = new SubjectCourseCreated.SubjectCourse(
            value.SubjectCourse.Id.ToGuid(),
            value.SubjectCourse.Title);

        return new SubjectCourseCreated.Notification(value.CorrelationId, subjectCourse);
    }
}