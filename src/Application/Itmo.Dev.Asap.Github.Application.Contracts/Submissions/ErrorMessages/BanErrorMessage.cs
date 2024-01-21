using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class BanErrorMessage : IErrorMessage
{
    private const string Title = "Error occured while processing Ban command";
    private const string IssuerNotFoundMessage = $"{Title} \n Issuer was not found";
    private const string AssignmentNotFoundMessage = $"{Title} \n Assignment was not found";
    private const string StudentNotFoundMessage = $"{Title} \n Current repository is not attached to any student";

    private readonly string _message;

    private BanErrorMessage(string message)
    {
        _message = message;
    }

    public static BanErrorMessage IssuerNotFound => new(IssuerNotFoundMessage);
    public static BanErrorMessage AssignmentNotFound => new(AssignmentNotFoundMessage);
    public static BanErrorMessage StudentNotFound => new(StudentNotFoundMessage);

    public async Task WriteMessage(IPullRequestCommentEventNotifier notifier)
    {
        await notifier.SendCommentToPullRequest(_message);
    }

    public override string ToString()
    {
        return _message;
    }
}