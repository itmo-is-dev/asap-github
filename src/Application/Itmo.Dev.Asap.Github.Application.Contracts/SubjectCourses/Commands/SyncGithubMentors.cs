using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;

internal static class SyncGithubMentors
{
    public record Command(long OrganizationId) : IRequest<Response>;

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record SubjectCourseNotFound : Response;

        public sealed record AssociationNotFound : Response;
    }
}