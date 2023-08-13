using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;
using Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;
using MediatR;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Controllers;

public class GithubManagementController : GithubManagementService.GithubManagementServiceBase
{
    private readonly IMediator _mediator;

    public GithubManagementController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Empty> ForceOrganizationUpdate(
        ForceOrganizationUpdateRequest request,
        ServerCallContext context)
    {
        UpdateSubjectCourseOrganization.Command command = request.MapTo();
        await _mediator.Send(command, context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> ForceAllOrganizationsUpdate(ForceAllOrganizationsUpdateRequest request, ServerCallContext context)
    {
        var command = new UpdateSubjectCourseOrganizations.Command();
        await _mediator.Send(command, context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> ForceMentorSync(ForceMentorSyncRequest request, ServerCallContext context)
    {
        SyncGithubMentors.Command command = request.MapTo();
        await _mediator.Send(command, context.CancellationToken);

        return new Empty();
    }
}