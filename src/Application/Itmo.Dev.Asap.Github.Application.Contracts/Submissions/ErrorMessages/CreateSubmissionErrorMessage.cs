using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class CreateSubmissionErrorMessage : IErrorMessage
{
    private const string Title = "Error occured while processing Create Submission command";
    private const string IssuerNotFoundMessage = $"{Title} \n Issuer was not found";
    private const string AssignmentNotFoundMessage = $"{Title} \n Assignment was not found";
    private const string StudentNotFoundMessage = $"{Title} \n Current repository is not attached to any student";
    private const string UnexpectedMessage = $"{Title} \n Operation produces unexpected result";

    private const string UnauthorizedMessage =
        $"{Title} \n You are not authorized to create submission at this repository";

    private readonly string _message;

    private CreateSubmissionErrorMessage(string message)
    {
        _message = message;
    }

    public static readonly CreateSubmissionErrorMessage IssuerNotFound = new(IssuerNotFoundMessage);

    public static readonly CreateSubmissionErrorMessage AssignmentNotFound = new(AssignmentNotFoundMessage);

    public static readonly CreateSubmissionErrorMessage StudentNotFound = new(StudentNotFoundMessage);

    public static readonly CreateSubmissionErrorMessage Unauthorized = new(UnauthorizedMessage);

    public static readonly CreateSubmissionErrorMessage Unexpected = new(UnexpectedMessage);

    public async Task WriteMessage(IPullRequestCommentEventNotifier notifier)
    {
        await notifier.SendCommentToPullRequest(_message);
    }

    public override string ToString()
    {
        return _message;
    }
}