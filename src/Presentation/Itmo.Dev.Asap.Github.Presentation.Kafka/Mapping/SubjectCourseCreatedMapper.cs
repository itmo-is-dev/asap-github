using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Notifications;
using Itmo.Dev.Asap.Kafka;
using Riok.Mapperly.Abstractions;

namespace Itmo.Dev.Asap.Github.Presentation.Kafka.Mapping;

[Mapper]
internal static partial class SubjectCourseCreatedMapper
{
    public static partial SubjectCourseCreated.Notification MapTo(this SubjectCourseCreatedValue value);
}