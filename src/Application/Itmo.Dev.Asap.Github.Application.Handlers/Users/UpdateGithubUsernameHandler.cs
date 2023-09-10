using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Domain.Users;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.Users.Commands.UpdateGithubUsername;

namespace Itmo.Dev.Asap.Github.Application.Handlers.Users;

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
        GithubUserModel? githubUser = await _githubUserService
            .FindByUsernameAsync(request.GithubUsername, cancellationToken);

        if (githubUser is null)
            return new Response.GithubUserNotFound();

        GithubUser? user = await _persistenceContext.Users.FindByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            user = new GithubUser(request.UserId, githubUser.Id);
            _persistenceContext.Users.Add(user);
        }
        else
        {
            user.GithubId = githubUser.Id;
            _persistenceContext.Users.Update(user);
        }

        await _persistenceContext.CommitAsync(cancellationToken);

        return new Response.Success();
    }
}