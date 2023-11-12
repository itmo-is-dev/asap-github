using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using Itmo.Dev.Asap.Github.Application.Specifications;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.Users.Commands.UpdateGithubUsernames;

namespace Itmo.Dev.Asap.Github.Application.Users;

internal class UpdateGithubUsernameHandler : IRequestHandler<Command, Response>
{
    private readonly IGithubUserService _githubUserService;
    private readonly IPersistenceContext _persistenceContext;

    public UpdateGithubUsernameHandler(IGithubUserService githubUserService, IPersistenceContext persistenceContext)
    {
        _githubUserService = githubUserService;
        _persistenceContext = persistenceContext;
    }

    public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
    {
        string[] usernames = request.Models
            .Select(x => x.GithubUsername)
            .Distinct()
            .ToArray();

        if (request.Models.Count != usernames.Length)
        {
            IEnumerable<string> duplicates = request.Models
                .Select(x => x.GithubUsername)
                .GroupBy(x => x, (u, others) => (useranme: u, count: others.Count()))
                .Where(x => x.count > 1)
                .Select(x => x.useranme);

            return new Response.DuplicateUsernames(duplicates);
        }

        Dictionary<string, GithubUserModel?> githubUsers = await usernames
            .ToAsyncEnumerable()
            .SelectAwait(async u => (u, user: await _githubUserService.FindByUsernameAsync(u, cancellationToken)))
            .ToDictionaryAsync(x => x.u, x => x.user, cancellationToken);

        if (githubUsers.Count(x => x.Value is not null) != usernames.Length)
        {
            IEnumerable<string> notFoundUsernames = githubUsers
                .Where(x => x.Value is null)
                .Select(x => x.Key);

            return new Response.GithubUsersNotFound(notFoundUsernames);
        }

        foreach (Command.Model model in request.Models)
        {
            GithubUser? user = await _persistenceContext.Users.FindByIdAsync(model.UserId, cancellationToken);
            GithubUserModel githubUser = githubUsers[model.GithubUsername]!;

            if (user is null)
            {
                user = new GithubUser(model.UserId, githubUser.Id);
                _persistenceContext.Users.Add(user);
            }
            else
            {
                user = user with
                {
                    GithubId = githubUser.Id,
                };

                _persistenceContext.Users.Update(user);
            }
        }

        await _persistenceContext.CommitAsync(cancellationToken);

        return new Response.Success();
    }
}