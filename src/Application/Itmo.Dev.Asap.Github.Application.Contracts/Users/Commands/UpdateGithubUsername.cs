using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Users.Commands;

internal static class UpdateGithubUsername
{
    public record Command(Guid UserId, string GithubUsername) : IRequest<Response>;

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record GithubUserNotFound : Response;
    }
}