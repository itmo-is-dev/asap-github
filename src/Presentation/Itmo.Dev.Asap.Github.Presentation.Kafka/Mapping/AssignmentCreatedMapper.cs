using Itmo.Dev.Asap.Github.Application.Dto.Assignments;
using Itmo.Dev.Asap.Kafka;
using Riok.Mapperly.Abstractions;

namespace Itmo.Dev.Asap.Github.Presentation.Kafka.Mapping;

[Mapper]
internal static partial class AssignmentCreatedMapper
{
    public static partial AssignmentDto MapTo(this AssignmentCreatedValue value);
}