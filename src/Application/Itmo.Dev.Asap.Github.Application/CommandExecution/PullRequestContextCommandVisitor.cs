using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.Submissions;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.CommandVisitors;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;
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
        GithubUser? issuer = await _context.Users.FindByGithubIdAsync(_pullRequest.SenderId);
        if (issuer is null)
        {
            return new SubmissionCommandResult.Failure(ActivateErrorMessage.IssuerNotFound());
        }

        GithubSubmission? submission = await _context.Submissions.FindSubmissionForPullRequestAsync(_pullRequest);

        if (submission is null)
        {
            return new SubmissionCommandResult.Failure(ActivateErrorMessage.SubmissionNotFound());
        }

        await _submissionService.ActivateSubmissionAsync(issuer.Id, submission.Id, default);

        const string message = "Submission activated successfully.";
        await _eventNotifier.SendCommentToPullRequest(message);

        return new SubmissionCommandResult.Success();
    }

    public async Task<SubmissionCommandResult> VisitAsync(BanCommand command)
    {
        GithubUser? issuer = await _context.Users.FindByGithubIdAsync(_pullRequest.SenderId);

        if (issuer is null)
        {
            return new SubmissionCommandResult.Failure(BanErrorMessage.IssuerNotFound());
        }

        GithubAssignment? assignment = await _context.Assignments.FindAssignmentForPullRequestAsync(_pullRequest);

        if (assignment is null)
        {
            return new SubmissionCommandResult.Failure(BanErrorMessage.AssignmentNotFound());
        }

        GithubSubjectCourseStudent? student =
            await _context.SubjectCourses.FindSubjectCourseStudentByRepositoryId(_pullRequest.RepositoryId, default);

        if (student is null)
        {
            return new SubmissionCommandResult.Failure(BanErrorMessage.StudentNotFound());
        }

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
        GithubUser? issuer = await _context.Users.FindByGithubIdAsync(_pullRequest.SenderId);

        if (issuer is null)
        {
            return new SubmissionCommandResult.Failure(UnbanErrorMessage.IssuerNotFound());
        }

        GithubAssignment? assignment = await _context.Assignments.FindAssignmentForPullRequestAsync(_pullRequest);

        if (assignment is null)
        {
            return new SubmissionCommandResult.Failure(UnbanErrorMessage.AssignmentNotFound());
        }

        GithubSubjectCourseStudent? student = await _context.SubjectCourses
            .FindSubjectCourseStudentByRepositoryId(_pullRequest.RepositoryId, default);

        if (student is null)
        {
            return new SubmissionCommandResult.Failure(UnbanErrorMessage.StudentNotFound());
        }

        UnbanSubmissionResult result = await _submissionService.UnbanSubmissionAsync(
            issuer.Id,
            student.User.Id,
            assignment.Id,
            default,
            default);

        switch (result)
        {
            case UnbanSubmissionResult.Success success:
            {
                string message = $"Submission {success.Submission.Code} successfully unbanned.";
                await _eventNotifier.SendCommentToPullRequest(message);

                return new SubmissionCommandResult.Success();
            }

            case UnbanSubmissionResult.Unauthorized:
                return new SubmissionCommandResult.Failure(
                    UnbanErrorMessage.Unauthorized());
            case UnbanSubmissionResult.InvalidMove invalidMove:
                return new SubmissionCommandResult.Failure(
                    UnbanErrorMessage.InvalidMove(invalidMove.SourceState.ToString()));
            default:
                return new SubmissionCommandResult.Failure(
                    UnbanErrorMessage.Unexpected());
        }
    }

    public async Task<SubmissionCommandResult> VisitAsync(CreateSubmissionCommand command)
    {
        GithubUser? issuer = await _context.Users.FindByGithubIdAsync(_pullRequest.SenderId);

        if (issuer is null)
        {
            return new SubmissionCommandResult.Failure(CreateSubmissionErrorMessage.IssuerNotFound());
        }

        GithubAssignment? assignment = await _context.Assignments.FindAssignmentForPullRequestAsync(_pullRequest);

        if (assignment is null)
        {
            return new SubmissionCommandResult.Failure(CreateSubmissionErrorMessage.AssignmentNotFound());
        }

        GithubSubjectCourseStudent? student = await _context.SubjectCourses
            .FindSubjectCourseStudentByRepositoryId(_pullRequest.RepositoryId, default);

        if (student is null)
        {
            return new SubmissionCommandResult.Failure(CreateSubmissionErrorMessage.StudentNotFound());
        }

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
                _pullRequest.PullRequestId,
                _pullRequest.CommitHash);

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
            return new SubmissionCommandResult.Failure(CreateSubmissionErrorMessage.Unauthorized());
        }

        return new SubmissionCommandResult.Failure(CreateSubmissionErrorMessage.Unexpected());
    }

    public async Task<SubmissionCommandResult> VisitAsync(DeactivateCommand command)
    {
        GithubUser? issuer = await _context.Users.FindByGithubIdAsync(_pullRequest.SenderId);
        if (issuer is null)
        {
            return new SubmissionCommandResult.Failure(DeactivateErrorMessage.IssuerNotFound());
        }

        GithubSubmission? submission = await _context.Submissions.FindSubmissionForPullRequestAsync(_pullRequest);

        if (submission is null)
        {
            return new SubmissionCommandResult.Failure(DeactivateErrorMessage.SubmissionNotFound());
        }

        await _submissionService.DeactivateSubmissionAsync(issuer.Id, submission.Id, default);

        const string message = "Submission deactivated successfully.";
        await _eventNotifier.SendCommentToPullRequest(message);

        return new SubmissionCommandResult.Success();
    }

    public async Task<SubmissionCommandResult> VisitAsync(DeleteCommand command)
    {
        GithubUser? issuer = await _context.Users.FindByGithubIdAsync(_pullRequest.SenderId);
        if (issuer is null)
        {
            return new SubmissionCommandResult.Failure(DeleteErrorMessage.IssuerNotFound());
        }

        GithubSubmission? submission = await _context.Submissions.FindSubmissionForPullRequestAsync(_pullRequest);

        if (submission is null)
        {
            return new SubmissionCommandResult.Failure(DeleteErrorMessage.SubmissionNotFound());
        }

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
            return new SubmissionCommandResult.Failure(DeleteErrorMessage.UnsuccessfulDeletion(e.Message));
        }
    }

    public async Task<SubmissionCommandResult> VisitAsync(HelpCommand command)
    {
        await _eventNotifier.SendCommentToPullRequest(HelpCommand.HelpString);
        return new SubmissionCommandResult.Success();
    }

    public async Task<SubmissionCommandResult> VisitAsync(MarkReviewedCommand command)
    {
        GithubUser? issuer = await _context.Users.FindByGithubIdAsync(_pullRequest.SenderId);
        if (issuer is null)
        {
            return new SubmissionCommandResult.Failure(MarkReviewedErrorMessage.IssuerNotFound());
        }

        GithubSubmission? submission = await _context.Submissions.FindSubmissionForPullRequestAsync(_pullRequest);

        if (submission is null)
        {
            return new SubmissionCommandResult.Failure(MarkReviewedErrorMessage.SubmissionNotFound());
        }

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
        GithubUser? issuer = await _context.Users.FindByGithubIdAsync(_pullRequest.SenderId);
        if (issuer is null)
        {
            return new SubmissionCommandResult.Failure(RateErrorMessage.IssuerNotFound());
        }

        GithubSubmission? submission = await _context.Submissions.FindSubmissionForPullRequestAsync(_pullRequest);

        if (submission is null)
        {
            return new SubmissionCommandResult.Failure(RateErrorMessage.SubmissionNotFound());
        }

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

        return result is RateSubmissionResult.Failure f
            ? new SubmissionCommandResult.Failure(RateErrorMessage.WithMessage(f.Message))
            : (SubmissionCommandResult)new SubmissionCommandResult.Failure(RateErrorMessage.Unexpected());
    }

    public async Task<SubmissionCommandResult> VisitAsync(UpdateCommand command)
    {
        GithubUser? issuer = await _context.Users.FindByGithubIdAsync(_pullRequest.SenderId);

        if (issuer is null)
        {
            return new SubmissionCommandResult.Failure(UpdateErrorMessage.IssuerNotFound());
        }

        GithubAssignment? assignment = await _context.Assignments.FindAssignmentForPullRequestAsync(_pullRequest);

        if (assignment is null)
        {
            return new SubmissionCommandResult.Failure(UpdateErrorMessage.AssignmentNotFound());
        }

        GithubSubjectCourseStudent? student =
            await _context.SubjectCourses.FindSubjectCourseStudentByRepositoryId(_pullRequest.RepositoryId, default);

        if (student is null)
        {
            return new SubmissionCommandResult.Failure(UpdateErrorMessage.StudentNotFound());
        }

        UpdateSubmissionResult result = await _submissionService.UpdateSubmissionAsync(
            issuer.Id,
            student.User.Id,
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
            ? new SubmissionCommandResult.Failure(UpdateErrorMessage.WithMessage(f.ErrorMessage))
            : new SubmissionCommandResult.Failure(UpdateErrorMessage.Unexpected());
    }
}