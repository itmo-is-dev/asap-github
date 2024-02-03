using Itmo.Dev.Asap.Github.Presentation.Kafka.ConsumerHandlers;
using Itmo.Dev.Asap.Kafka;
using Itmo.Dev.Platform.Kafka.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Itmo.Dev.Asap.Github.Presentation.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaPresentation(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        const string consumerKey = "Presentation:Kafka:Consumers";
        const string producerKey = "Presentation:Kafka:Producers";

        string group = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;

        collection.AddPlatformKafka(builder => builder
            .ConfigureOptions(configuration.GetSection("Presentation:Kafka"))
            .AddConsumer(b => b
                .WithKey<AssignmentCreatedKey>()
                .WithValue<AssignmentCreatedValue>()
                .WithConfiguration(
                    configuration.GetSection($"{consumerKey}:AssignmentCreated"),
                    c => c.WithGroup(group))
                .DeserializeKeyWithProto()
                .DeserializeValueWithProto()
                .HandleWith<AssignmentCreatedHandler>())
            .AddConsumer(b => b
                .WithKey<SubjectCourseCreatedKey>()
                .WithValue<SubjectCourseCreatedValue>()
                .WithConfiguration(
                    configuration.GetSection($"{consumerKey}:SubjectCourseCreated"),
                    c => c.WithGroup(group))
                .DeserializeKeyWithProto()
                .DeserializeValueWithProto()
                .HandleWith<SubjectCourseCreatedHandler>())
            .AddProducer(b => b
                .WithKey<SubmissionDataKey>()
                .WithValue<SubmissionDataValue>()
                .WithConfiguration(configuration.GetSection($"{producerKey}:SubmissionData"))
                .SerializeKeyWithProto()
                .SerializeValueWithProto()));

        collection.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<IAssemblyMarker>());

        return collection;
    }
}