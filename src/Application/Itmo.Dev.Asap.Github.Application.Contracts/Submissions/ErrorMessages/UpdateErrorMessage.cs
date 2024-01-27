using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class UpdateErrorMessage : IErrorMessage
{
    private const string Title = "Error occured while processing Update command";
    private const string IssuerNotFoundMessage = $"{Title} \n Issuer was not found";
    private const string AssignmentNotFoundMessage = $"{Title} \n Assignment was not found";
    private const string StudentNotFoundMessage = $"{Title} \n Current repository is not attached to any student";
    private const string UnexpectedMessage = $"{Title} \n Failed to rate submission";

    private readonly string _message;

    private UpdateErrorMessage(string message)
    {
        _message = message;
    }

    public static readonly UpdateErrorMessage IssuerNotFound = new(IssuerNotFoundMessage);

    public static readonly UpdateErrorMessage AssignmentNotFound = new(AssignmentNotFoundMessage);

    public static readonly UpdateErrorMessage StudentNotFound = new(StudentNotFoundMessage);

    public static readonly UpdateErrorMessage Unexpected = new(UnexpectedMessage);

    public static UpdateErrorMessage WithMessage(string errorMessage)
        => new($"{Title} \n {errorMessage}");

    public async Task WriteMessage(IPullRequestCommentEventNotifier notifier)
    {
        await notifier.SendCommentToPullRequest(_message);
    }

    public override string ToString()
    {
        return _message;
    }
}