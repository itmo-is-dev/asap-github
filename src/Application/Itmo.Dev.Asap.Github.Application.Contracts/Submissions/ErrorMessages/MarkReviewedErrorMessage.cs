namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class MarkReviewedErrorMessage : ErrorMessage
{
    private const string Title = "Error occured while processing Mark Reviewed command";

    private MarkReviewedErrorMessage(string message)
    {
        Message = message;
    }

    public static MarkReviewedErrorMessage IssuerNotFound()
    {
        string message = $"{Title} \n Issuer was not found";
        return new MarkReviewedErrorMessage(message);
    }

    public static MarkReviewedErrorMessage SubmissionNotFound()
    {
        string message = $"{Title} \n Submission was not found";
        return new MarkReviewedErrorMessage(message);
    }

    protected override string Message { get; }
}