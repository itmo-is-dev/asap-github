using Itmo.Dev.Asap.Github.Application.Core.Exceptions;
using Itmo.Dev.Asap.Github.Application.Core.Services.Submissions;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.Dto.PullRequests;
using Itmo.Dev.Asap.Github.Application.Dto.Submissions;
using Itmo.Dev.Asap.Github.Application.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Commands.CommandVisitors;
using Itmo.Dev.Asap.Github.Commands.Models;
using Itmo.Dev.Asap.Github.Commands.SubmissionCommands;
using Itmo.Dev.Asap.Github.Domain.Assignments;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using Itmo.Dev.Asap.Github.Domain.Submissions;
using Itmo.Dev.Asap.Github.Domain.Users;

namespace Itmo.Dev.Asap.Github.Application.CommandExecution;

public class PullRequestContextCommandVisitor : ISubmissionCommandVisitor
{
    private readonly ISubmissionService _submissionService;
    private readonly PullRequestDto _pullRequest;
    private readonly IPullRequestEventNotifier _eventNotifier;
    private readonly IPersistenceContext _context;

    public PullRequestContextCommandVisitor(
        ISubmissionService submissionService,
        PullRequestDto pullRequest,
        IPullRequestEventNotifier eventNotifier,
        IPersistenceContext context)
    {
        _submissionService = submissionService;
        _pullRequest = pullRequest;
        _eventNotifier = eventNotifier;
        _context = context;
    }

    public async Task<SubmissionCommandResult> VisitAsync(ActivateCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubSubmission submission = await _context.Submissions.GetSubmissionForPullRequestAsync(_pullRequest);

        try
        {
            await _submissionService.ActivateSubmissionAsync(issuer.Id, submission.Id, default);

            const string message = "Submission activated successfully.";
            await _eventNotifier.SendCommentToPullRequest(message);

            return new SubmissionCommandResult.Success();
        }
        catch (AsapCoreException e)
        {
            return new SubmissionCommandResult.Failure(e.Message);
        }
    }

    public async Task<SubmissionCommandResult> VisitAsync(BanCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubAssignment assignment = await _context.Assignments.GetAssignmentForPullRequestAsync(_pullRequest);

        GithubSubjectCourseStudent? student = await _context.SubjectCourses
            .FindSubjectCourseStudentByRepositoryId(_pullRequest.RepositoryId, default);

        if (student is null)
            return new SubmissionCommandResult.Failure("Current repository is not attached to any student");

        try
        {
            SubmissionDto submission = await _submissionService.BanSubmissionAsync(
                issuer.Id,
                student.User.Id,
                assignment.Id,
                default,
                default);

            string message = $"Submission {submission.Code} successfully banned.";
            await _eventNotifier.SendCommentToPullRequest(message);

            return new SubmissionCommandResult.Success();
        }
        catch (AsapCoreException e)
        {
            return new SubmissionCommandResult.Failure(e.Message);
        }
    }

    public async Task<SubmissionCommandResult> VisitAsync(CreateSubmissionCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubAssignment assignment = await _context.Assignments.GetAssignmentForPullRequestAsync(_pullRequest);

        GithubSubjectCourseStudent? student = await _context.SubjectCourses
            .FindSubjectCourseStudentByRepositoryId(_pullRequest.RepositoryId, default);

        if (student is null)
            return new SubmissionCommandResult.Failure("Current repository is not attached to any student");

        try
        {
            SubmissionDto submission = await _submissionService.CreateSubmissionAsync(
                issuer.Id,
                student.User.Id,
                assignment.Id,
                _pullRequest.Payload,
                default);

            var githubSubmission = new GithubSubmission(
                submission.Id,
                assignment.Id,
                student.User.Id,
                submission.SubmissionDate,
                _pullRequest.OrganizationId,
                _pullRequest.RepositoryId,
                _pullRequest.PullRequestId);

            _context.Submissions.Add(githubSubmission);
            await _context.CommitAsync(default);

            string message = $"""
        Submission created.
        {submission.ToDisplayString()}
        """;

            await _eventNotifier.SendCommentToPullRequest(message);

            return new SubmissionCommandResult.Success();
        }
        catch (AsapCoreException e)
        {
            return new SubmissionCommandResult.Failure(e.Message);
        }
    }

    public async Task<SubmissionCommandResult> VisitAsync(DeactivateCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubSubmission submission = await _context.Submissions.GetSubmissionForPullRequestAsync(_pullRequest);

        try
        {
            await _submissionService.DeactivateSubmissionAsync(issuer.Id, submission.Id, default);

            const string message = "Submission deactivated successfully.";
            await _eventNotifier.SendCommentToPullRequest(message);

            return new SubmissionCommandResult.Success();
        }
        catch (AsapCoreException e)
        {
            return new SubmissionCommandResult.Failure(e.Message);
        }
    }

    public async Task<SubmissionCommandResult> VisitAsync(DeleteCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubSubmission submission = await _context.Submissions.GetSubmissionForPullRequestAsync(_pullRequest);

        try
        {
            await _submissionService.DeleteSubmissionAsync(
                issuer.Id,
                submission.Id,
                default);

            const string message = "Submission deleted successfully.";
            await _eventNotifier.SendCommentToPullRequest(message);

            return new SubmissionCommandResult.Success();
        }
        catch (Exception e)
        {
            return new SubmissionCommandResult.Failure(e.Message);
        }
    }

    public async Task<SubmissionCommandResult> VisitAsync(HelpCommand command)
    {
        await _eventNotifier.SendCommentToPullRequest(HelpCommand.HelpString);
        return new SubmissionCommandResult.Success();
    }

    public async Task<SubmissionCommandResult> VisitAsync(MarkReviewedCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubSubmission submission = await _context.Submissions.GetSubmissionForPullRequestAsync(_pullRequest);

        try
        {
            await _submissionService.MarkSubmissionAsReviewedAsync(
                issuer.Id,
                submission.Id,
                default);

            const string message = "Submission marked as reviewed.";
            await _eventNotifier.SendCommentToPullRequest(message);

            return new SubmissionCommandResult.Success();
        }
        catch (AsapCoreException e)
        {
            return new SubmissionCommandResult.Failure(e.Message);
        }
    }

    public async Task<SubmissionCommandResult> VisitAsync(RateCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubSubmission submission = await _context.Submissions.GetSubmissionForPullRequestAsync(_pullRequest);

        try
        {
            RateSubmissionResult result = await _submissionService.RateSubmissionAsync(
                issuer.Id,
                submission.Id,
                command.RatingPercent,
                command.ExtraPoints,
                default);

            if (result is RateSubmissionResult.Success s)
            {
                string message = $"""
                Submission rated.
                {s.Submission.ToDisplayString()}
                """;

                await _eventNotifier.SendCommentToPullRequest(message);

                return new SubmissionCommandResult.Success();
            }

            if (result is RateSubmissionResult.Failure f)
                return new SubmissionCommandResult.Failure(f.Message);

            return new SubmissionCommandResult.Failure("Failed to rate submission");
        }
        catch (AsapCoreException e)
        {
            return new SubmissionCommandResult.Failure(e.Message);
        }
    }

    public async Task<SubmissionCommandResult> VisitAsync(UpdateCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubAssignment assignment = await _context.Assignments.GetAssignmentForPullRequestAsync(_pullRequest);

        GithubSubjectCourseStudent? user = await _context.SubjectCourses
            .FindSubjectCourseStudentByRepositoryId(_pullRequest.RepositoryId, default);

        if (user is null)
            return new SubmissionCommandResult.Failure("Current repository is not attached to any student");

        try
        {
            SubmissionRateDto submission = await _submissionService.UpdateSubmissionAsync(
                issuer.Id,
                user.User.Id,
                assignment.Id,
                command.SubmissionCode,
                command.GetDate(),
                command.RatingPercent,
                command.ExtraPoints,
                default);

            string message = $"""
            Submission rated.
            {submission.ToDisplayString()}
            """;

            await _eventNotifier.SendCommentToPullRequest(message);

            return new SubmissionCommandResult.Success();
        }
        catch (AsapCoreException e)
        {
            return new SubmissionCommandResult.Failure(e.Message);
        }
    }
}