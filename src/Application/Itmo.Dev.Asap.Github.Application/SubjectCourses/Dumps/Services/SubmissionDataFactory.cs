using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Results;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Abstractions.Storage;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Models;
using Itmo.Dev.Asap.Github.Common.Exceptions;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Services;

public class SubmissionDataFactory
{
    private readonly IGithubContentService _contentService;
    private readonly IStorageService _storageService;
    private readonly SubjectCourseDumpOptions _options;

    public SubmissionDataFactory(
        IGithubContentService contentService,
        IStorageService storageService,
        IOptions<SubjectCourseDumpOptions> options)
    {
        _contentService = contentService;
        _storageService = storageService;
        _options = options.Value;
    }

    public async Task<CreateSubmissionDataResult> CreateSubmissionDataAsync(
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
            var error = new SubjectCourseDumpError(
                $"Repository data for {organization.Name}/{repository.Name} ({submission.CommitHash}) not found");

            return new CreateSubmissionDataResult.Failure(error);
        }

        if (contentResult is GetRepositoryContentResult.UnexpectedError unexpectedError)
        {
            var error = new SubjectCourseDumpError(
                $"Encountered unexpected error while fetching content for {organization.Name}/{repository.Name} ({submission.CommitHash}): {unexpectedError.Message}");

            return new CreateSubmissionDataResult.Failure(error);
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

        return new CreateSubmissionDataResult.Success(submissionData);
    }
}