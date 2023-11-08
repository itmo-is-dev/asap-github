using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;
using Itmo.Dev.Asap.Github.Common.Tools;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;

internal static class GithubManagementServiceMapper
{
    public static UpdateSubjectCourseOrganization.Command MapTo(this ForceOrganizationUpdateRequest request)
        => new UpdateSubjectCourseOrganization.Command(request.SubjectCourseId.ToGuid());

    public static SyncGithubMentors.Command MapTo(this ForceMentorSyncRequest request)
        => new SyncGithubMentors.Command(request.OrganizationId);
}