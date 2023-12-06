using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Models.Assignments;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Services;

public class SubmissionHashProvider
{
    private readonly IGithubSubmissionLocatorService _submissionLocatorService;
    private readonly IPersistenceContext _context;

    public SubmissionHashProvider(IGithubSubmissionLocatorService submissionLocatorService, IPersistenceContext context)
    {
        _submissionLocatorService = submissionLocatorService;
        _context = context;
    }

    public async Task<string?> FindOrUpdateCommitHashAsync(
        GithubSubmission submission,
        GithubOrganizationModel organization,
        GithubRepositoryModel repository,
        GithubAssignment assignment,
        HashSet<long> mentorIds,
        CancellationToken cancellationToken)
    {
        string? hash = submission.CommitHash;

        if (hash is not null)
            return hash;

        hash = await _submissionLocatorService.FindSubmissionCommitHash(
            organization,
            repository,
            assignment.BranchName,
            mentorIds,
            cancellationToken);

        if (hash is not null)
            _context.Submissions.UpdateCommitHash(submission.Id, hash);

        return hash;
    }
}