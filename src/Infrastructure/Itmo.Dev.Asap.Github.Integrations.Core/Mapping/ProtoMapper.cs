using Google.Protobuf.WellKnownTypes;
using Itmo.Dev.Asap.Core;
using Itmo.Dev.Asap.Core.Models;
using Itmo.Dev.Asap.Github.Application.Core.Models;
using Itmo.Dev.Asap.Github.Application.Dto.Submissions;
using Itmo.Dev.Asap.Github.Application.Dto.Users;
using Riok.Mapperly.Abstractions;

namespace Itmo.Dev.Asap.Github.Integrations.Core.Mapping;

[Mapper]
internal static partial class ProtoMapper
{
    public static partial UserDto ToDto(this User user);

    public static partial StudentDto ToDto(this Student student);

    public static partial SubmissionDto ToDto(this Submission submission);

    [MapProperty(nameof(SubmissionRate.SubmissionId), nameof(SubmissionRateDto.Id))]
    public static partial SubmissionRateDto ToDto(this SubmissionRate submissionRate);

    private static DateTime ToDateTime(Timestamp timestamp)
        => timestamp.ToDateTime();
}