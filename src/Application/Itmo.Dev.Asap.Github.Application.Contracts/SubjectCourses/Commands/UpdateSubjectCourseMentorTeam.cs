using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;

internal static class UpdateSubjectCourseMentorTeam
{
    public record Command(Guid SubjectCourseId, long MentorTeamId) : IRequest<Response>;

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record SubjectCourseNotFound : Response;

        public sealed record MentorTeamNotFound : Response;
    }
}