using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries;
using Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;
using Itmo.Dev.Asap.Github.SubjectCourses;
using MediatR;
using ProvisionSubjectCourseCommand =
    Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands.ProvisionSubjectCourse;

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
            ProvisionSubjectCourseCommand.Response.Success => new Empty(),

            ProvisionSubjectCourseCommand.Response.OrganizationAlreadyBound => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Specified organization already bound to another subject course")),

            ProvisionSubjectCourseCommand.Response.OrganizationNotFound => throw new RpcException(new Status(
                StatusCode.NotFound,
                "Organization with specified id not found")),

            ProvisionSubjectCourseCommand.Response.TemplateRepositoryNotFound => throw new RpcException(new Status(
                StatusCode.NotFound,
                "Specified template repository not found")),

            ProvisionSubjectCourseCommand.Response.TemplateRepositoryNotMarkedTemplate
                => throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    "Specified template repository is not marked as template")),

            ProvisionSubjectCourseCommand.Response.MentorTeamNotFound => throw new RpcException(new Status(
                StatusCode.NotFound,
                "Specified mentor team not found")),

            _ => throw new RpcException(new Status(StatusCode.Internal, "Operation finished unexpectedly")),
        };
    }

    public override async Task<Empty> UpdateMentorTeam(UpdateMentorTeamRequest request, ServerCallContext context)
    {
        UpdateSubjectCourseMentorTeam.Command command = request.MapTo();
        UpdateSubjectCourseMentorTeam.Response response = await _mediator.Send(command, context.CancellationToken);

        return response switch
        {
            UpdateSubjectCourseMentorTeam.Response.Success => new Empty(),

            UpdateSubjectCourseMentorTeam.Response.SubjectCourseNotFound => throw new RpcException(new Status(
                StatusCode.NotFound,
                "Specified subject course not found")),

            UpdateSubjectCourseMentorTeam.Response.MentorTeamNotFound => throw new RpcException(new Status(
                StatusCode.NotFound,
                "Specified mentor team not found")),

            _ => throw new RpcException(new Status(StatusCode.Internal, "Operation finished unexpectedly")),
        };
    }

    public override async Task<FindByIdsResponse> FindByIds(FindByIdsRequest request, ServerCallContext context)
    {
        FindSubjectCoursesByIds.Query query = request.MapTo();
        FindSubjectCoursesByIds.Response response = await _mediator.Send(query, context.CancellationToken);

        return response.MapFrom();
    }
}