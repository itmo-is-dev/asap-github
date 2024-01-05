namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class ActivateErrorMessage : ErrorMessage
{
    private const string Title = "Error occured while processing Activate command";

    private ActivateErrorMessage(string message)
    {
        Message = message;
    }

    public static ActivateErrorMessage IssuerNotFound()
    {
        string message = $"{Title} \n Issuer was not found";
        return new ActivateErrorMessage(message);
    }

    public static ActivateErrorMessage SubmissionNotFound()
    {
        string message = $"{Title} \n Submission was not found";
        return new ActivateErrorMessage(message);
    }

    protected override string Message { get; }
}
