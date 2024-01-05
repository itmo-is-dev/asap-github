using Itmo.Dev.Asap.Github.Application.Models.PullRequests;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents;

internal static class PullRequestChangesRequested
{
    public record Command(PullRequestDto PullRequest) : IRequest<Response>;

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record IssuerNotFound : Response;

        public sealed record SubmissionNotFound : Response;
    }
}