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
        await _mediator.Send(command, context.CancellationToken);

        return new Empty();
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