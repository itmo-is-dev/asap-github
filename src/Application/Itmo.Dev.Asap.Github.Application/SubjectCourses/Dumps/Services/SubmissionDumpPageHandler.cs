using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Models.Assignments;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Models;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Services;

public class SubmissionDumpPageHandler
{
    private readonly IPersistenceContext _context;
    private readonly StudentRepositoryProvider _repositoryProvider;
    private readonly SubmissionHashProvider _submissionHashProvider;
    private readonly SubmissionDataFactory _submissionDataFactory;
    private readonly ILogger<SubmissionDumpPageHandler> _logger;

    public SubmissionDumpPageHandler(
        IPersistenceContext context,
        StudentRepositoryProvider repositoryProvider,
        SubmissionHashProvider submissionHashProvider,
        SubmissionDataFactory submissionDataFactory,
        ILogger<SubmissionDumpPageHandler> logger)
    {
        _context = context;
        _repositoryProvider = repositoryProvider;
        _submissionHashProvider = submissionHashProvider;
        _submissionDataFactory = submissionDataFactory;
        _logger = logger;
    }

    public async Task<DumpPageResult> DumpPageAsync(
        BackgroundTaskId backgroundTaskId,
        GithubSubjectCourse subjectCourse,
        GithubOrganizationModel organization,
        IReadOnlyCollection<GithubSubmission> submissions,
        HashSet<long> mentorIds,
        CancellationToken cancellationToken)
    {
        var data = new List<GithubSubmissionData>();

        var assignmentsQuery = GithubAssignmentQuery.Build(builder => builder
            .WithIds(submissions.Select(x => x.AssignmentId)));

        Dictionary<Guid, GithubAssignment> assignments = await _context.Assignments
            .QueryAsync(assignmentsQuery, cancellationToken)
            .ToDictionaryAsync(assignment => assignment.Id, cancellationToken);

        Dictionary<Guid, GithubRepositoryModel> repositories = await _repositoryProvider
            .GetStudentRepositories(
                subjectCourse,
                userIds: submissions.Select(x => x.UserId),
                cancellationToken)
            .ToDictionaryAsync(
                keySelector: tuple => tuple.StudentId,
                elementSelector: tuple => tuple.Repository,
                cancellationToken);

        if (repositories.Count != submissions.Select(x => x.UserId).Distinct().Count())
        {
            var error = new SubjectCourseDumpError("Could not find some repositories");
            return new DumpPageResult.Failure(error);
        }

        foreach (GithubSubmission submission in submissions)
        {
            GithubRepositoryModel repository = repositories[submission.UserId];
            GithubAssignment assignment = assignments[submission.AssignmentId];

            string? hash = await _submissionHashProvider.FindOrUpdateCommitHashAsync(
                submission,
                organization,
                repository,
                assignment,
                mentorIds,
                cancellationToken);

            if (hash is null)
            {
                _logger.LogInformation("Failed to find hash for submission = {SubmissionId}", submission.Id);
                continue;
            }

            CreateSubmissionDataResult result = await _submissionDataFactory.CreateSubmissionDataAsync(
                backgroundTaskId,
                organization,
                repository,
                submission,
                hash,
                cancellationToken);

            if (result is CreateSubmissionDataResult.Failure failure)
                return new DumpPageResult.Failure(failure.Error);

            if (result is CreateSubmissionDataResult.Success success)
                data.Add(success.Data);
        }

        return new DumpPageResult.Success(data);
    }
}