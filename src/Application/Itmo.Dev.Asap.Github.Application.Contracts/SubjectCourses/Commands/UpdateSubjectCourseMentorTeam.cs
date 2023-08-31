using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;

internal static class UpdateSubjectCourseMentorTeam
{
    public record Command(Guid SubjectCourseId, long MentorTeamId) : IRequest;
}