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

        collection.AddKafka(builder => builder
            .ConfigureOptions(b => b.BindConfiguration("Presentation:Kafka"))
            .AddConsumer<AssignmentCreatedKey, AssignmentCreatedValue>(selector => selector
                .HandleWith<AssignmentCreatedHandler>()
                .DeserializeKeyWithProto()
                .DeserializeValueWithProto()
                .UseNamedOptionsConfiguration(
                    "AssignmentCreated",
                    configuration.GetSection($"{consumerKey}:AssignmentCreated"),
                    c => c.WithGroup(group)))
            .AddConsumer<SubjectCourseCreatedKey, SubjectCourseCreatedValue>(selector => selector
                .HandleWith<SubjectCourseCreatedHandler>()
                .DeserializeKeyWithProto()
                .DeserializeValueWithProto()
                .UseNamedOptionsConfiguration(
                    "SubjectCourseCreated",
                    configuration.GetSection($"{consumerKey}:SubjectCourseCreated"),
                    c => c.WithGroup(group)))
            .AddProducer<SubmissionDataKey, SubmissionDataValue>(selector => selector
                .SerializeKeyWithProto()
                .SerializeValueWithProto()
                .UseNamedOptionsConfiguration(
                    "SubmissionData",
                    configuration.GetSection($"{producerKey}:SubmissionData"))));

        collection.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<IAssemblyMarker>());

        return collection;
    }
}