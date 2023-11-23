using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Results;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Abstractions.Storage;
using Itmo.Dev.Asap.Github.Application.Models.Assignments;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Common.Exceptions;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps;

public class SubjectCourseDumpTask : IBackgroundTask<
    SubjectCourseDumpMetadata,
    SubjectCourseDumpExecutionMetadata,
    EmptyExecutionResult,
    SubjectCourseDumpError>
{
    private readonly IPersistenceContext _context;
    private readonly SubjectCourseDumpOptions _options;
    private readonly IGithubOrganizationService _organizationService;
    private readonly IGithubRepositoryService _repositoryService;
    private readonly IGithubContentService _contentService;
    private readonly IStorageService _storageService;
    private readonly IGithubSubmissionLocatorService _submissionLocatorService;

    public SubjectCourseDumpTask(
        IPersistenceContext context,
        IOptions<SubjectCourseDumpOptions> options,
        IGithubOrganizationService organizationService,
        IGithubRepositoryService repositoryService,
        IGithubContentService contentService,
        IStorageService storageService,
        IGithubSubmissionLocatorService submissionLocatorService)
    {
        _context = context;
        _organizationService = organizationService;
        _repositoryService = repositoryService;
        _contentService = contentService;
        _storageService = storageService;
        _submissionLocatorService = submissionLocatorService;
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

        do
        {
            SubjectCourseDumpError? error = await DumpPageAsync(
                executionContext.Id,
                subjectCourse,
                subjectCourseOrganization,
                executionMetadata,
                cancellationToken);

            if (error is not null)
                return new BackgroundTaskExecutionResult<EmptyExecutionResult, SubjectCourseDumpError>.Failure(error);
        }
        while (executionMetadata.Key is not null);

        return new BackgroundTaskExecutionResult<EmptyExecutionResult, SubjectCourseDumpError>.Success(
            EmptyExecutionResult.Value);
    }

    private async Task<SubjectCourseDumpError?> DumpPageAsync(
        BackgroundTaskId backgroundTaskId,
        GithubSubjectCourse subjectCourse,
        GithubOrganizationModel organization,
        SubjectCourseDumpExecutionMetadata executionMetadata,
        CancellationToken cancellationToken)
    {
        var submissionsQuery = FirstGithubSubmissionQuery.Build(x => x
            .WithUserId(executionMetadata.Key?.StudentId)
            .WithAssignmentId(executionMetadata.Key?.AssignmentId)
            .WithPageSize(_options.PageSize));

        GithubSubmission[] submissions = await _context.Submissions
            .QueryFirstSubmissionsAsync(submissionsQuery, cancellationToken)
            .ToArrayAsync(cancellationToken);

        var assignmentsQuery = GithubAssignmentQuery.Build(builder => builder
            .WithIds(submissions.Select(x => x.AssignmentId)));

        Dictionary<Guid, GithubAssignment> assignments = await _context.Assignments
            .QueryAsync(assignmentsQuery, cancellationToken)
            .ToDictionaryAsync(assignment => assignment.Id, cancellationToken);

        Dictionary<Guid, GithubRepositoryModel> repositories = await GetStudentRepositories(
                subjectCourse,
                userIds: submissions.Select(x => x.UserId),
                cancellationToken)
            .ToDictionaryAsync(
                keySelector: tuple => tuple.UserId,
                elementSelector: tuple => tuple.Repository,
                cancellationToken);

        if (repositories.Count != submissions.Select(x => x.UserId).Distinct().Count())
            return new SubjectCourseDumpError("Could not find some repositories");

        foreach (GithubSubmission submission in submissions)
        {
            GithubRepositoryModel repository = repositories[submission.UserId];
            GithubAssignment assignment = assignments[submission.AssignmentId];

            string? hash = await FindOrUpdateCommitHashAsync(
                submission,
                organization,
                repository,
                assignment,
                cancellationToken);

            if (hash is null)
            {
                return new SubjectCourseDumpError(
                    $"Could not find hash for {organization.Name}/{repository.Name} ({assignment.BranchName})");
            }

            SubjectCourseDumpError? error = await CreateSubmissionDataAsync(
                backgroundTaskId,
                organization,
                repository,
                submission,
                hash,
                cancellationToken);

            if (error is not null)
                return error;
        }

        await _context.CommitAsync(cancellationToken);

        executionMetadata.Key = submissions.Length.Equals(_options.PageSize)
            ? MapToKey(submissions[^1])
            : null;

        return null;
    }

    private async Task<string?> FindOrUpdateCommitHashAsync(
        GithubSubmission submission,
        GithubOrganizationModel organization,
        GithubRepositoryModel repository,
        GithubAssignment assignment,
        CancellationToken cancellationToken)
    {
        string? hash = submission.CommitHash;

        if (hash is not null)
            return hash;

        hash = await _submissionLocatorService.FindSubmissionCommitHash(
            organization,
            repository.Name,
            assignment.BranchName,
            cancellationToken);

        if (hash is not null)
            _context.Submissions.UpdateCommitHash(submission.Id, hash);

        return hash;
    }

    private async Task<SubjectCourseDumpError?> CreateSubmissionDataAsync(
        BackgroundTaskId backgroundTaskId,
        GithubOrganizationModel organization,
        GithubRepositoryModel repository,
        GithubSubmission submission,
        string hash,
        CancellationToken cancellationToken)
    {
        GetRepositoryContentResult contentResult = await _contentService.GetRepositoryContentAsync(
            organization.Id,
            repository.Id,
            hash,
            cancellationToken);

        if (contentResult is GetRepositoryContentResult.NotFound)
        {
            return new SubjectCourseDumpError(
                $"Repository data for {organization.Name}/{repository.Name} ({submission.CommitHash}) not found");
        }

        if (contentResult is GetRepositoryContentResult.UnexpectedError unexpectedError)
        {
            return new SubjectCourseDumpError(
                $"Encountered unexpected error while fetching content for {organization.Name}/{repository.Name} ({submission.CommitHash}): {unexpectedError.Message}");
        }

        if (contentResult is not GetRepositoryContentResult.Success success)
            throw new UnexpectedOperationResultException { Value = contentResult };

        await using Stream content = success.Content;
        StoredData storedData = await _storageService.StoreAsync(_options.BucketName, content, cancellationToken);

        var submissionData = new GithubSubmissionData(
            submission.Id,
            submission.UserId,
            submission.AssignmentId,
            backgroundTaskId.Value,
            storedData.Link);

        _context.Submissions.AddData(submissionData);

        return null;
    }

    private async IAsyncEnumerable<(Guid UserId, GithubRepositoryModel Repository)> GetStudentRepositories(
        GithubSubjectCourse subjectCourse,
        IEnumerable<Guid> userIds,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseStudentQuery.Build(x => x
            .WithSubjectCourseId(subjectCourse.Id)
            .WithUserIds(userIds));

        IAsyncEnumerable<GithubSubjectCourseStudent> students = _context.SubjectCourses
            .QueryStudentsAsync(query, cancellationToken);

        await foreach (GithubSubjectCourseStudent student in students)
        {
            GithubRepositoryModel? repository = await _repositoryService.FindByIdAsync(
                subjectCourse.OrganizationId,
                student.RepositoryId,
                cancellationToken);

            if (repository is not null)
                yield return (student.User.Id, repository);
        }
    }

// Static members should precede non static members
#pragma warning disable SA1204

    private static SubjectCourseDumpExecutionMetadata.StudentAssignmentKey MapToKey(GithubSubmission submission)
    {
        return new SubjectCourseDumpExecutionMetadata.StudentAssignmentKey(submission.UserId, submission.AssignmentId);
    }
}