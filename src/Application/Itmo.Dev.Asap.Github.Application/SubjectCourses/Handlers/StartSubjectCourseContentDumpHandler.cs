using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.Locking;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands.StartSubjectCourseContentDump;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Handlers;

internal class StartSubjectCourseContentDumpHandler : IRequestHandler<Command, Response>
{
    private readonly IBackgroundTaskRepository _backgroundTaskRepository;
    private readonly IBackgroundTaskRunner _backgroundTaskRunner;
    private readonly IPersistenceContext _context;
    private readonly ILockingService _lockingService;

    public StartSubjectCourseContentDumpHandler(
        IBackgroundTaskRepository backgroundTaskRepository,
        IBackgroundTaskRunner backgroundTaskRunner,
        IPersistenceContext context,
        ILockingService lockingService)
    {
        _backgroundTaskRepository = backgroundTaskRepository;
        _backgroundTaskRunner = backgroundTaskRunner;
        _context = context;
        _lockingService = lockingService;
    }

    public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
    {
        GithubSubjectCourse? subjectCourse = await _context.SubjectCourses
            .FindByIdAsync(request.SubjectCourseId, cancellationToken);

        if (subjectCourse is null)
            return new Response.SubjectCourseNotFound();

        var metadata = new SubjectCourseDumpMetadata(subjectCourse.Id);

        await using ILockHandle lockHandle = await _lockingService.AcquireAsync(metadata, cancellationToken);

        var backgroundTaskQuery = BackgroundTaskQuery.Build(builder => builder
            .WithName(SubjectCourseDumpTask.Name)
            .WithMetadata(metadata)
            .WithState(BackgroundTaskState.Pending)
            .WithState(BackgroundTaskState.Enqueued)
            .WithPageSize(1));

        bool isAlreadyRunning = await _backgroundTaskRepository
            .QueryAsync(backgroundTaskQuery, cancellationToken)
            .AnyAsync(cancellationToken);

        if (isAlreadyRunning)
            return new Response.AlreadyRunning();

        BackgroundTaskId taskId = await _backgroundTaskRunner
            .StartBackgroundTask
            .WithMetadata(metadata)
            .WithExecutionMetadata(new SubjectCourseDumpExecutionMetadata())
            .RunWithAsync<SubjectCourseDumpTask>(cancellationToken);

        return new Response.Success(taskId.Value);
    }
}