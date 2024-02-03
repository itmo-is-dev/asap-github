using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Kafka;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;
using MediatR;
using SubmissionDataAdded = Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Notifications.SubmissionDataAdded;
using SubmissionDataCollectionFinished =
    Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Notifications.SubmissionDataCollectionFinished;

namespace Itmo.Dev.Asap.Github.Presentation.Kafka.ProducerHandlers;

internal class SubmissionDataHandler :
    INotificationHandler<SubmissionDataAdded.Notification>,
    INotificationHandler<SubmissionDataCollectionFinished.Notification>
{
    private readonly IKafkaMessageProducer<SubmissionDataKey, SubmissionDataValue> _producer;

    public SubmissionDataHandler(IKafkaMessageProducer<SubmissionDataKey, SubmissionDataValue> producer)
    {
        _producer = producer;
    }

    public async Task Handle(SubmissionDataAdded.Notification notification, CancellationToken cancellationToken)
    {
        IAsyncEnumerable<KafkaProducerMessage<SubmissionDataKey, SubmissionDataValue>> messages = notification.Data
            .Select(data => new KafkaProducerMessage<SubmissionDataKey, SubmissionDataValue>(
                new SubmissionDataKey { TaskId = data.TaskId },
                new SubmissionDataValue { SubmissionDataAdded = Map(data) }))
            .ToAsyncEnumerable();

        await _producer.ProduceAsync(messages, cancellationToken);
    }

    public async Task Handle(
        SubmissionDataCollectionFinished.Notification notification,
        CancellationToken cancellationToken)
    {
        var message = new KafkaProducerMessage<SubmissionDataKey, SubmissionDataValue>(
            new SubmissionDataKey { TaskId = notification.TaskId },
            new SubmissionDataValue { SubmissionDataCollectionFinished = new() });

        await _producer.ProduceAsync(message, cancellationToken);
    }

    private static Asap.Kafka.SubmissionDataAdded Map(GithubSubmissionData data)
    {
        return new Asap.Kafka.SubmissionDataAdded
        {
            UserId = data.UserId.ToString(),
            AssignmentId = data.AssignmentId.ToString(),
            SubmissionId = data.SubmissionId.ToString(),
            FileLink = data.FileLink,
        };
    }
}