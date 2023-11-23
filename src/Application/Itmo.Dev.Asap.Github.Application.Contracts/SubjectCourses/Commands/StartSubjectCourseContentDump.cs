using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;

internal static class StartSubjectCourseContentDump
{
    public record Command(Guid SubjectCourseId) : IRequest<Response>;

    public abstract record Response
    {
        private Response() { }

        public sealed record Success(long TaskId) : Response;

        public sealed record AlreadyRunning : Response;

        public sealed record SubjectCourseNotFound : Response;
    }
}