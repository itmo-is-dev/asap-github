namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class DeactivateErrorMessage : ErrorMessage
{
    private const string Title = "Error occured while processing Deactivate command";

    private DeactivateErrorMessage(string message)
    {
        Message = message;
    }

    public static DeactivateErrorMessage IssuerNotFound()
    {
        string message = $"{Title} \n Issuer was not found";
        return new DeactivateErrorMessage(message);
    }

    public static DeactivateErrorMessage SubmissionNotFound()
    {
        string message = $"{Title} \n Submission was not found";
        return new DeactivateErrorMessage(message);
    }

    protected override string Message { get; }
}
