using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;
using Riok.Mapperly.Abstractions;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;

[Mapper]
internal static partial class GithubManagementServiceMapper
{
    public static partial UpdateSubjectCourseOrganization.Command MapTo(this ForceOrganizationUpdateRequest request);

    public static partial SyncGithubMentors.Command MapTo(this ForceMentorSyncRequest request);
}