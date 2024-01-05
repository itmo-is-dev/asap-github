namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class DeleteErrorMessage : ErrorMessage
{
    private const string Title = "Error occured while processing Delete command";

    private DeleteErrorMessage(string message)
    {
        Message = message;
    }

    public static DeleteErrorMessage IssuerNotFound()
    {
        string message = $"{Title} \n Issuer was not found";
        return new DeleteErrorMessage(message);
    }

    public static DeleteErrorMessage SubmissionNotFound()
    {
        string message = $"{Title} \n Submission was not found";
        return new DeleteErrorMessage(message);
    }

    public static DeleteErrorMessage UnsuccessfulDeletion(string errorMessage)
    {
        string message = $"{Title} \n {errorMessage}";
        return new DeleteErrorMessage(message);
    }

    protected override string Message { get; }
}