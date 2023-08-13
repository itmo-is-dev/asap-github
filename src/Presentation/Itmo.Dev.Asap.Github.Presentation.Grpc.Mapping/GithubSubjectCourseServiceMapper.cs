using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries;
using Itmo.Dev.Asap.Github.SubjectCourses;
using Riok.Mapperly.Abstractions;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;

[Mapper]
internal static partial class GithubSubjectCourseServiceMapper
{
    public static partial ProvisionSubjectCourse.Command MapTo(this ProvisionSubjectCourseRequest request);

    public static partial UpdateSubjectCourseMentorTeam.Command MapTo(this UpdateMentorTeamRequest request);

    public static partial FindSubjectCoursesByIds.Query MapTo(this FindByIdsRequest request);

    public static partial FindByIdsResponse MapFrom(this FindSubjectCoursesByIds.Response response);
}