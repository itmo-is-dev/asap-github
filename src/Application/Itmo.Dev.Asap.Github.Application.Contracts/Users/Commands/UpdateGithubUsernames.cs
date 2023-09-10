using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Users.Commands;

internal static class UpdateGithubUsernames
{
    public record Command(IReadOnlyCollection<Command.Model> Models) : IRequest<Response>
    {
        public sealed record Model(Guid UserId, string GithubUsername);
    }

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record DuplicateUsernames(IEnumerable<string> Duplicates) : Response;

        public sealed record GithubUsersNotFound(IEnumerable<string> Usernames) : Response;
    }
}