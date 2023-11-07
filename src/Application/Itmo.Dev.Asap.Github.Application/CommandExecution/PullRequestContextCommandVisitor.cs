using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.Submissions;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.CommandVisitors;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Models;
using Itmo.Dev.Asap.Github.Application.Models.Assignments;
using Itmo.Dev.Asap.Github.Application.Models.PullRequests;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using Itmo.Dev.Asap.Github.Application.Specifications;

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

        await _submissionService.ActivateSubmissionAsync(issuer.Id, submission.Id, default);

        const string message = "Submission activated successfully.";
        await _eventNotifier.SendCommentToPullRequest(message);

        return new SubmissionCommandResult.Success();
    }

    public async Task<SubmissionCommandResult> VisitAsync(BanCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubAssignment assignment = await _context.Assignments.GetAssignmentForPullRequestAsync(_pullRequest);

        GithubSubjectCourseStudent? student = await _context.SubjectCourses
            .FindSubjectCourseStudentByRepositoryId(_pullRequest.RepositoryId, default);

        if (student is null)
            return new SubmissionCommandResult.Failure("Current repository is not attached to any student");

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

    public async Task<SubmissionCommandResult> VisitAsync(UnbanCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubAssignment assignment = await _context.Assignments.GetAssignmentForPullRequestAsync(_pullRequest);

        GithubSubjectCourseStudent? student = await _context.SubjectCourses
            .FindSubjectCourseStudentByRepositoryId(_pullRequest.RepositoryId, default);

        if (student is null)
            return new SubmissionCommandResult.Failure("Current repository is not attached to any student");

        UnbanSubmissionResult result = await _submissionService.UnbanSubmissionAsync(
            issuer.Id,
            student.User.Id,
            assignment.Id,
            default,
            default);

        if (result is UnbanSubmissionResult.Success success)
        {
            string message = $"Submission {success.Submission.Code} successfully unbanned.";
            await _eventNotifier.SendCommentToPullRequest(message);

            return new SubmissionCommandResult.Success();
        }

        if (result is UnbanSubmissionResult.Unauthorized)
        {
            string message = "You are not authorized to create submission at this repository";
            return new SubmissionCommandResult.Failure(message);
        }

        if (result is UnbanSubmissionResult.InvalidMove invalidMove)
        {
            string message = $"Cannot unban submission in {invalidMove.SourceState} state";
            return new SubmissionCommandResult.Failure(message);
        }

        return new SubmissionCommandResult.Failure("Operation produces unexpected result");
    }

    public async Task<SubmissionCommandResult> VisitAsync(CreateSubmissionCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubAssignment assignment = await _context.Assignments.GetAssignmentForPullRequestAsync(_pullRequest);

        GithubSubjectCourseStudent? student = await _context.SubjectCourses
            .FindSubjectCourseStudentByRepositoryId(_pullRequest.RepositoryId, default);

        if (student is null)
            return new SubmissionCommandResult.Failure("Current repository is not attached to any student");

        CreateSubmissionResult result = await _submissionService.CreateSubmissionAsync(
            issuer.Id,
            student.User.Id,
            assignment.Id,
            _pullRequest.Payload,
            default);

        if (result is CreateSubmissionResult.Success s)
        {
            SubmissionDto submission = s.Submission;

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

        if (result is CreateSubmissionResult.Unauthorized)
        {
            string message = "You are not authorized to create submission at this repository";
            return new SubmissionCommandResult.Failure(message);
        }

        return new SubmissionCommandResult.Failure("Operation produces unexpected result");
    }

    public async Task<SubmissionCommandResult> VisitAsync(DeactivateCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubSubmission submission = await _context.Submissions.GetSubmissionForPullRequestAsync(_pullRequest);

        await _submissionService.DeactivateSubmissionAsync(issuer.Id, submission.Id, default);

        const string message = "Submission deactivated successfully.";
        await _eventNotifier.SendCommentToPullRequest(message);

        return new SubmissionCommandResult.Success();
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

        await _submissionService.MarkSubmissionAsReviewedAsync(
            issuer.Id,
            submission.Id,
            default);

        const string message = "Submission marked as reviewed.";
        await _eventNotifier.SendCommentToPullRequest(message);

        return new SubmissionCommandResult.Success();
    }

    public async Task<SubmissionCommandResult> VisitAsync(RateCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubSubmission submission = await _context.Submissions.GetSubmissionForPullRequestAsync(_pullRequest);

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

    public async Task<SubmissionCommandResult> VisitAsync(UpdateCommand command)
    {
        GithubUser issuer = await _context.Users.GetForGithubIdAsync(_pullRequest.SenderId);
        GithubAssignment assignment = await _context.Assignments.GetAssignmentForPullRequestAsync(_pullRequest);

        GithubSubjectCourseStudent? user = await _context.SubjectCourses
            .FindSubjectCourseStudentByRepositoryId(_pullRequest.RepositoryId, default);

        if (user is null)
            return new SubmissionCommandResult.Failure("Current repository is not attached to any student");

        UpdateSubmissionResult result = await _submissionService.UpdateSubmissionAsync(
            issuer.Id,
            user.User.Id,
            assignment.Id,
            command.SubmissionCode,
            command.GetDate(),
            command.RatingPercent,
            command.ExtraPoints,
            default);

        if (result is UpdateSubmissionResult.Success s)
        {
            string message = $"""
                Submission rated.
                {s.Submission.ToDisplayString()}
                """;

            await _eventNotifier.SendCommentToPullRequest(message);

            return new SubmissionCommandResult.Success();
        }

        return result is UpdateSubmissionResult.Failure f
            ? new SubmissionCommandResult.Failure(f.ErrorMessage)
            : new SubmissionCommandResult.Failure("Failed to update submission");
    }
}