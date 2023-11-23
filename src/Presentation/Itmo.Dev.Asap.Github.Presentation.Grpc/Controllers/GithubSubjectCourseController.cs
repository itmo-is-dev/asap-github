using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Github.Common.Tools;
using Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;
using Itmo.Dev.Asap.Github.SubjectCourses;
using MediatR;
using Newtonsoft.Json;
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

    public override async Task<StartContentDumpResponse> StartContentDump(
        StartContentDumpRequest request,
        ServerCallContext context)
    {
        var command = new StartSubjectCourseContentDump.Command(request.SubjectCourseId.ToGuid());
        StartSubjectCourseContentDump.Response response = await _mediator.Send(command, context.CancellationToken);

        return response switch
        {
            StartSubjectCourseContentDump.Response.Success success => new StartContentDumpResponse
            {
                Success = new StartContentDumpResponse.Types.Success { TaskId = success.TaskId },
            },

            StartSubjectCourseContentDump.Response.AlreadyRunning => new StartContentDumpResponse
            {
                AlreadyRunning = new StartContentDumpResponse.Types.AlreadyRunning(),
            },

            StartSubjectCourseContentDump.Response.SubjectCourseNotFound => new StartContentDumpResponse
            {
                SubjectCourseNotFound = new StartContentDumpResponse.Types.SubjectCourseNotFound(),
            },

            _ => throw new RpcException(new Status(StatusCode.Internal, "Operation finished unexpectedly")),
        };
    }

    public override async Task<GetContentDumpResultResponse> GetContentDumpResult(
        GetContentDumpResultRequest request,
        ServerCallContext context)
    {
        GetSubjectCourseContentDumpResult.PageToken? pageToken = request.PageToken is null
            ? null
            : JsonConvert.DeserializeObject<GetSubjectCourseContentDumpResult.PageToken>(request.PageToken);

        var query = new GetSubjectCourseContentDumpResult.Query(
            request.TaskId,
            pageToken,
            request.PageSize);

        GetSubjectCourseContentDumpResult.Response response = await _mediator.Send(query, context.CancellationToken);

        return response switch
        {
            GetSubjectCourseContentDumpResult.Response.Success success => new GetContentDumpResultResponse
            {
                Success = new GetContentDumpResultResponse.Types.Success
                {
                    PageToken = success.PageToken is null ? null : JsonConvert.SerializeObject(success.PageToken),
                    Data = { success.Data.Select(MapToData) },
                },
            },

            GetSubjectCourseContentDumpResult.Response.Failure failure => new GetContentDumpResultResponse
            {
                Failure = new GetContentDumpResultResponse.Types.Failure
                {
                    Message = failure.Message,
                },
            },

            GetSubjectCourseContentDumpResult.Response.TaskNotCompleted => new GetContentDumpResultResponse
            {
                TaskNotCompleted = new GetContentDumpResultResponse.Types.TaskNotCompleted(),
            },

            GetSubjectCourseContentDumpResult.Response.TaskNotFound => new GetContentDumpResultResponse
            {
                TaskNotFound = new GetContentDumpResultResponse.Types.TaskNotFound(),
            },

            _ => throw new RpcException(new Status(StatusCode.Internal, "Operation finished unexpectedly")),
        };
    }

    private static Models.GithubSubmissionData MapToData(GithubSubmissionData data)
    {
        return new Models.GithubSubmissionData
        {
            SubmissionId = data.SubmissionId.ToString(),
            UserId = data.UserId.ToString(),
            AssignmentId = data.AssignmentId.ToString(),
            TaskId = data.TaskId,
            FileLink = data.FileLink,
        };
    }
}