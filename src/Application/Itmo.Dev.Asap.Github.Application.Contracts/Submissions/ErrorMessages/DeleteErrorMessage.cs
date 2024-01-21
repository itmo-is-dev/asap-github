using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class DeleteErrorMessage : IErrorMessage
{
    private const string Title = "Error occured while processing Delete command";
    private const string IssuerNotFoundMessage = $"{Title} \n Issuer was not found";
    private const string SubmissionNotFoundMessage = $"{Title} \n Submission was not found";

    private readonly string _message;

    private DeleteErrorMessage(string message)
    {
        _message = message;
    }

    public static DeleteErrorMessage IssuerNotFound => new(IssuerNotFoundMessage);

    public static DeleteErrorMessage SubmissionNotFound => new(SubmissionNotFoundMessage);

    public static DeleteErrorMessage UnsuccessfulDeletion(string errorMessage)
    {
        return new DeleteErrorMessage($"{Title} \n {errorMessage}");
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