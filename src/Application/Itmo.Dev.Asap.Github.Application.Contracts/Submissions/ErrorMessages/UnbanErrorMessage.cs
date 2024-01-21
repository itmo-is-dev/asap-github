using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class UnbanErrorMessage : IErrorMessage
{
    private const string Title = "Error occured while processing Unban command";
    private const string IssuerNotFoundMessage = $"{Title} \n Issuer was not found";
    private const string AssignmentNotFoundMessage = $"{Title} \n Assignment was not found";
    private const string StudentNotFoundMessage = $"{Title} \n Current repository is not attached to any student";
    private const string UnexpectedMessage = $"{Title} \n Operation produces unexpected result";

    private const string UnauthorizedMessage =
        $"{Title} \n You are not authorized to create submission at this repository";

    private readonly string _message;

    private UnbanErrorMessage(string message)
    {
        _message = message;
    }

    public static UnbanErrorMessage IssuerNotFound => new(IssuerNotFoundMessage);

    public static UnbanErrorMessage AssignmentNotFound => new(AssignmentNotFoundMessage);

    public static UnbanErrorMessage StudentNotFound => new(StudentNotFoundMessage);

    public static UnbanErrorMessage Unauthorized => new(UnauthorizedMessage);

    public static UnbanErrorMessage Unexpected => new(UnexpectedMessage);

    public static UnbanErrorMessage InvalidMove(string sourceState)
    {
        return new UnbanErrorMessage($"{Title} \n Cannot unban submission in {sourceState} state");
    }

    public async Task WriteMessage(IPullRequestCommentEventNotifier notifier)
    {
        await notifier.SendCommentToPullRequest(_message);
    }

    public override string ToString()
    {
        return _message;
    }
}