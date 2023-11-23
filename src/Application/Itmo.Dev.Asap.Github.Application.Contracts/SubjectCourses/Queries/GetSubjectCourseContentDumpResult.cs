using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries;

internal static class GetSubjectCourseContentDumpResult
{
    public record Query(long TaskId, PageToken? PageToken, int PageSize) : IRequest<Response>;

    public record PageToken(Guid UserId, Guid AssignmentId);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success(IReadOnlyCollection<GithubSubmissionData> Data, PageToken? PageToken) : Response;

        public sealed record Failure(string Message) : Response;

        public sealed record TaskNotCompleted : Response;

        public sealed record TaskNotFound : Response;
    }
}