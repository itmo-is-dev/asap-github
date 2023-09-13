using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries;
using Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;
using Itmo.Dev.Asap.Github.SubjectCourses;
using MediatR;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Controllers;

public class GithubSubjectCourseController : GithubSubjectCourseService.GithubSubjectCourseServiceBase
{
    private readonly IMediator _mediator;

    public GithubSubjectCourseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Empty> ProvisionSubjectCourse(
        ProvisionSubjectCourseRequest request,
        ServerCallContext context)
    {
        ProvisionSubjectCourse.Command command = request.MapTo();
        ProvisionSubjectCourse.Response response = await _mediator.Send(command, context.CancellationToken);

        return response switch
        {
            Application.Contracts.SubjectCourses.Commands.ProvisionSubjectCourse.Response.Success => new Empty(),

            Application.Contracts.SubjectCourses.Commands.ProvisionSubjectCourse.Response.OrganizationAlreadyBound
                => throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    "Specified organization already bound to another subject course")),

            _ => throw new RpcException(new Status(StatusCode.Internal, "Operation finished unexpectedly")),
        };
    }

    public override async Task<Empty> UpdateMentorTeam(UpdateMentorTeamRequest request, ServerCallContext context)
    {
        UpdateSubjectCourseMentorTeam.Command command = request.MapTo();
        await _mediator.Send(command, context.CancellationToken);

        return new Empty();
    }

    public override async Task<FindByIdsResponse> FindByIds(FindByIdsRequest request, ServerCallContext context)
    {
        FindSubjectCoursesByIds.Query query = request.MapTo();
        FindSubjectCoursesByIds.Response response = await _mediator.Send(query, context.CancellationToken);

        return response.MapFrom();
    }
}