using Itmo.Dev.Asap.Github.Application.Core.Services;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Dto.Users;
using Itmo.Dev.Asap.Github.Application.Mapping;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Common.Exceptions;
using Itmo.Dev.Asap.Github.Common.Extensions;
using Itmo.Dev.Asap.Github.Domain.Users;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.Users.Commands.UpdateGithubUser;

namespace Itmo.Dev.Asap.Github.Application.Handlers.Users;

internal class UpdateGithubUserHandler : IRequestHandler<Command, Response>
{
    private readonly IUserService _asapUserService;
    private readonly IGithubUserService _githubUserService;
    private readonly IPersistenceContext _context;

    public UpdateGithubUserHandler(
        IUserService asapUserService,
        IGithubUserService githubUserService,
        IPersistenceContext context)
    {
        _asapUserService = asapUserService;
        _githubUserService = githubUserService;
        _context = context;
    }

    public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
    {
        var query = GithubUserQuery.Build(x => x
            .WithUsername(request.GithubUsername)
            .WithLimit(1));

        bool alreadyExists = await _context.Users
            .QueryAsync(query, cancellationToken)
            .AnyAsync(cancellationToken);

        if (alreadyExists)
            throw GithubUserException.UsernameCollision(request.GithubUsername).TaggedWithConflict();

        bool userExists = await _githubUserService.IsUserExistsAsync(request.GithubUsername, cancellationToken);

        if (userExists is false)
            throw GithubUserException.UserDoesNotExist(request.GithubUsername).TaggedWithNotFound();

        GithubUser? githubUser = await _context.Users
            .QueryAsync(GithubUserQuery.Build(x => x.WithId(request.UserId)), cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        if (githubUser is not null)
        {
            githubUser.Username = request.GithubUsername.ToLower();
            _context.Users.Update(githubUser);
        }
        else
        {
            UserDto user = await _asapUserService.GetByIdAsync(request.UserId, cancellationToken);
            githubUser = new GithubUser(user.Id, request.GithubUsername);

            _context.Users.Add(githubUser);
        }

        await _context.CommitAsync(cancellationToken);

        GithubUserDto dto = githubUser.ToDto();

        return new Response(dto);
    }
}