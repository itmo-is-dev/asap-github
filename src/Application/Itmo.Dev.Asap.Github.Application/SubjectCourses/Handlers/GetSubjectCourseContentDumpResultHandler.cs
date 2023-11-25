using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Queries.GetSubjectCourseContentDumpResult;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Handlers;

internal class GetSubjectCourseContentDumpResultHandler : IRequestHandler<Query, Response>
{
    private readonly IPersistenceContext _context;
    private readonly IBackgroundTaskRepository _backgroundTaskRepository;

    public GetSubjectCourseContentDumpResultHandler(
        IPersistenceContext context,
        IBackgroundTaskRepository backgroundTaskRepository)
    {
        _context = context;
        _backgroundTaskRepository = backgroundTaskRepository;
    }

    public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
    {
        var backgroundTaskQuery = BackgroundTaskQuery.Build(builder => builder
            .WithName(SubjectCourseDumpTask.Name)
            .WithId(new BackgroundTaskId(request.TaskId)));

        BackgroundTask? backgroundTask = await _backgroundTaskRepository
            .QueryAsync(backgroundTaskQuery, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        if (backgroundTask is null)
            return new Response.TaskNotFound();

        if (backgroundTask.State is BackgroundTaskState.Failed)
            return new Response.Failure((backgroundTask.Error as SubjectCourseDumpError)?.Message ?? string.Empty);

        if (backgroundTask.State is not BackgroundTaskState.Completed)
            return new Response.TaskNotCompleted();

        GithubSubmissionDataQuery.PageTokenModel? queryPageToken = request.PageToken is null
            ? null
            : new GithubSubmissionDataQuery.PageTokenModel(request.PageToken.UserId, request.PageToken.AssignmentId);

        var dataQuery = GithubSubmissionDataQuery.Build(builder => builder
            .WithTaskId(backgroundTask.Id.Value)
            .WithPageSize(request.PageSize)
            .WithPageToken(queryPageToken));

        GithubSubmissionData[] data = await _context.Submissions
            .QueryDataAsync(dataQuery, cancellationToken)
            .ToArrayAsync(cancellationToken);

        PageToken? pageToken = data.Length == request.PageSize
            ? MapToPageToken(data[^1])
            : null;

        return new Response.Success(data, pageToken);
    }

    private static PageToken MapToPageToken(GithubSubmissionData data)
        => new PageToken(data.UserId, data.AssignmentId);
}