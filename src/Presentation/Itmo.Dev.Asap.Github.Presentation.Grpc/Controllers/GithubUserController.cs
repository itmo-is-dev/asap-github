using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Itmo.Dev.Asap.Github.Application.Contracts.Users.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries;
using Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;
using Itmo.Dev.Asap.Github.Users;
using MediatR;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Controllers;

public class GithubUserController : GithubUserService.GithubUserServiceBase
{
    private readonly IMediator _mediator;

    public GithubUserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<FindByIdsResponse> FindByIds(FindByIdsRequest request, ServerCallContext context)
    {
        FindUsersByIds.Query query = request.MapTo();
        FindUsersByIds.Response response = await _mediator.Send(query, context.CancellationToken);

        return response.MapFrom();
    }

    public override async Task<Empty> UpdateUsername(UpdateUsernameRequest request, ServerCallContext context)
    {
        UpdateGithubUsername.Command command = request.MapTo();
        UpdateGithubUsername.Response response = await _mediator.Send(command, context.CancellationToken);

        return response switch
        {
            UpdateGithubUsername.Response.Success => new Empty(),

            UpdateGithubUsername.Response.GithubUserNotFound => throw new RpcException(
                new Status(StatusCode.NotFound, "Github user with specified username not found")),

            _ => throw new RpcException(new Status(StatusCode.Internal, "Operation ended unexpectedly")),
        };
    }
}