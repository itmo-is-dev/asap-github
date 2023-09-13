using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;

internal static class ProvisionSubjectCourse
{
    public record Command(
        string CorrelationId,
        long OrganizationId,
        long TemplateRepositoryId,
        long MentorTeamId) : IRequest<Response>;

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record OrganizationAlreadyBound : Response;
    }
}