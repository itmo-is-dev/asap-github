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
        const string assignmentKey = "Presentation:Kafka:Consumers:AssignmentCreated";
        const string subjectCourseKey = "Presentation:Kafka:Consumers:SubjectCourseCreated";

        string host = configuration.GetSection("Presentation:Kafka:Host").Get<string>() ?? string.Empty;
        string group = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;

        collection.AddKafkaConsumer<AssignmentCreatedKey, AssignmentCreatedValue>(selector => selector
            .HandleWith<AssignmentCreatedHandler>()
            .DeserializeKeyWithProto()
            .DeserializeValueWithProto()
            .UseNamedOptionsConfiguration(
                "AssignmentCreated",
                configuration.GetSection(assignmentKey),
                c => c.WithHost(host).WithGroup(group)));

        collection.AddKafkaConsumer<SubjectCourseCreatedKey, SubjectCourseCreatedValue>(selector => selector
            .HandleWith<SubjectCourseCreatedHandler>()
            .DeserializeKeyWithProto()
            .DeserializeValueWithProto()
            .UseNamedOptionsConfiguration(
                "SubjectCourseCreated",
                configuration.GetSection(subjectCourseKey),
                c => c.WithHost(host).WithGroup(group)));

        return collection;
    }
}