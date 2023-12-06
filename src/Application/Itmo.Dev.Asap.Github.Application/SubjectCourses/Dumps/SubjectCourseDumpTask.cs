using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.Submissions;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Notifications;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Models;
using Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Services;
using Itmo.Dev.Asap.Github.Common.Exceptions;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using MediatR;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps;

#pragma warning disable CA1506

public class SubjectCourseDumpTask : IBackgroundTask<
    SubjectCourseDumpMetadata,
    SubjectCourseDumpExecutionMetadata,
    EmptyExecutionResult,
    SubjectCourseDumpError>
{
    private readonly IPersistenceContext _context;
    private readonly SubjectCourseDumpOptions _options;
    private readonly IGithubOrganizationService _organizationService;
    private readonly ISubjectCourseService _subjectCourseService;
    private readonly ISubmissionService _submissionService;
    private readonly IPublisher _publisher;
    private readonly SubmissionDumpPageHandler _dumpPageHandler;

    public SubjectCourseDumpTask(
        IPersistenceContext context,
        IOptions<SubjectCourseDumpOptions> options,
        IGithubOrganizationService organizationService,
        ISubjectCourseService subjectCourseService,
        ISubmissionService submissionService,
        IPublisher publisher,
        SubmissionDumpPageHandler dumpPageHandler)
    {
        _context = context;
        _organizationService = organizationService;
        _subjectCourseService = subjectCourseService;
        _submissionService = submissionService;
        _publisher = publisher;
        _dumpPageHandler = dumpPageHandler;
        _options = options.Value;
    }

    public static string Name => nameof(SubjectCourseDumpTask);

    public async Task<BackgroundTaskExecutionResult<EmptyExecutionResult, SubjectCourseDumpError>> ExecuteAsync(
        BackgroundTaskExecutionContext<SubjectCourseDumpMetadata, SubjectCourseDumpExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        Guid subjectCourseId = executionContext.Metadata.SubjectCourseId;
        SubjectCourseDumpExecutionMetadata executionMetadata = executionContext.ExecutionMetadata;

        GithubSubjectCourse subjectCourse = await _context.SubjectCourses
            .GetByIdAsync(subjectCourseId, cancellationToken);

        GithubOrganizationModel? subjectCourseOrganization = await _organizationService
            .FindByIdAsync(subjectCourse.OrganizationId, cancellationToken);

        if (subjectCourseOrganization is null)
        {
            var error = new SubjectCourseDumpError("Organization for subject course is not found");
            return new BackgroundTaskExecutionResult<EmptyExecutionResult, SubjectCourseDumpError>.Failure(error);
        }

        IEnumerable<Guid> mentorIds = await _subjectCourseService
            .GetSubjectCourseMentors(subjectCourseId, cancellationToken);

        HashSet<long> mentors = await _context.Users
            .QueryAsync(GithubUserQuery.Build(x => x.WithIds(mentorIds)), cancellationToken)
            .Select(x => x.GithubId)
            .ToHashSetAsync(cancellationToken);

        do
        {
            QueryFirstSubmissionsResponse submissionResponse = await _submissionService.QueryFirstCompletedSubmissions(
                subjectCourseId,
                _options.PageSize,
                executionMetadata.SubmissionPageToken,
                cancellationToken);

            IEnumerable<Guid> submissionIds = submissionResponse.Submissions.Select(x => x.SubmissionId);

            GithubSubmission[] submissions = await _context.Submissions
                .QueryAsync(GithubSubmissionQuery.Build(x => x.WithIds(submissionIds)), cancellationToken)
                .ToArrayAsync(cancellationToken);

            DumpPageResult result = await _dumpPageHandler.DumpPageAsync(
                executionContext.Id,
                subjectCourse,
                subjectCourseOrganization,
                submissions,
                mentors,
                cancellationToken);

            if (result is DumpPageResult.Failure failure)
            {
                return new BackgroundTaskExecutionResult<EmptyExecutionResult, SubjectCourseDumpError>
                    .Failure(failure.Error);
            }

            if (result is not DumpPageResult.Success success)
                throw new UnexpectedOperationResultException();

            _context.Submissions.AddData(success.Data);
            await _context.CommitAsync(cancellationToken);

            var notification = new SubmissionDataAdded.Notification(success.Data);
            await _publisher.Publish(notification, default);

            executionMetadata.SubmissionPageToken = submissionResponse.PageToken;
        }
        while (executionMetadata.SubmissionPageToken is not null);

        await _publisher.Publish(new SubmissionDataCollectionFinished.Notification(executionContext.Id.Value), default);

        EmptyExecutionResult value = EmptyExecutionResult.Value;
        return new BackgroundTaskExecutionResult<EmptyExecutionResult, SubjectCourseDumpError>.Success(value);
    }
}