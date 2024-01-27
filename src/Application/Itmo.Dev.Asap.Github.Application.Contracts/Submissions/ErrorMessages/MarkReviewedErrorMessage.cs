using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class MarkReviewedErrorMessage : IErrorMessage
{
    private const string Title = "Error occured while processing Mark Reviewed command";
    private const string IssuerNotFoundMessage = $"{Title} \n Issuer was not found";
    private const string SubmissionNotFoundMessage = $"{Title} \n Submission was not found";

    private readonly string _message;

    private MarkReviewedErrorMessage(string message)
    {
        _message = message;
    }

    public static readonly MarkReviewedErrorMessage IssuerNotFound = new(IssuerNotFoundMessage);

    public static readonly MarkReviewedErrorMessage SubmissionNotFound = new(SubmissionNotFoundMessage);

    public async Task WriteMessage(IPullRequestCommentEventNotifier notifier)
    {
        await notifier.SendCommentToPullRequest(_message);
    }

    public override string ToString()
    {
        return _message;
    }
}