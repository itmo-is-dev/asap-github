using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Github.Common.Tools;
using Itmo.Dev.Asap.Github.SubjectCourses;
using GithubSubjectCourse = Itmo.Dev.Asap.Github.Models.GithubSubjectCourse;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;

internal static class GithubSubjectCourseServiceMapper
{
    public static ProvisionSubjectCourse.Command MapTo(this ProvisionSubjectCourseRequest request)
    {
        return new ProvisionSubjectCourse.Command(
            request.CorrelationId,
            request.OrganizationId,
            request.TemplateRepositoryId,
            request.MentorTeamId);
    }

    public static UpdateSubjectCourseMentorTeam.Command MapTo(this UpdateMentorTeamRequest request)
        => new UpdateSubjectCourseMentorTeam.Command(request.SubjectCourseId.ToGuid(), request.MentorTeamId);

    public static FindSubjectCoursesByIds.Query MapTo(this FindByIdsRequest request)
        => new FindSubjectCoursesByIds.Query(request.SubjectCourseIds.Select(x => x.ToGuid()));

    public static FindByIdsResponse MapFrom(this FindSubjectCoursesByIds.Response response)
    {
        return new FindByIdsResponse
        {
            SubjectCourses = { response.SubjectCourses.Select(MapFrom) },
        };
    }

    private static GithubSubjectCourse MapFrom(this EnrichedGithubSubjectCourse subjectCourse)
    {
        return new GithubSubjectCourse
        {
            Id = subjectCourse.Id.ToString(),
            OrganizationId = subjectCourse.OrganizationId,
            OrganizationName = subjectCourse.OrganizationName,
            TemplateRepositoryId = subjectCourse.TemplateRepositoryId,
            TemplateRepositoryName = subjectCourse.TemplateRepositoryName,
            MentorTeamId = subjectCourse.MentorTeamId,
            MentorTeamName = subjectCourse.MentorTeamName,
        };
    }
}