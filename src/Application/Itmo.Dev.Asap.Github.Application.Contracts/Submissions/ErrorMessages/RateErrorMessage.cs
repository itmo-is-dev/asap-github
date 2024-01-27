using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Notifications;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class RateErrorMessage : IErrorMessage
{
    private const string Title = "Error occured while processing Rate command";
    private const string IssuerNotFoundMessage = $"{Title} \n Issuer was not found";
    private const string SubmissionNotFoundMessage = $"{Title} \n Submission was not found";
    private const string UnexpectedMessage = $"{Title} \n Failed to rate submission";

    private readonly string _message;

    private RateErrorMessage(string message)
    {
        _message = message;
    }

    public static readonly RateErrorMessage IssuerNotFound = new(IssuerNotFoundMessage);

    public static readonly RateErrorMessage SubmissionNotFound = new(SubmissionNotFoundMessage);

    public static readonly RateErrorMessage Unexpected = new(UnexpectedMessage);

    public static RateErrorMessage WithMessage(string errorMessage)
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