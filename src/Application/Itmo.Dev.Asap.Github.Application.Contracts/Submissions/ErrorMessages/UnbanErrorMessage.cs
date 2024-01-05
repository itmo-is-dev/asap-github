namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class UnbanErrorMessage : ErrorMessage
{
    private const string Title = "Error occured while processing Unban command";

    private UnbanErrorMessage(string message)
    {
        Message = message;
    }

    public static UnbanErrorMessage IssuerNotFound()
    {
        string message = $"{Title} \n Issuer was not found";
        return new UnbanErrorMessage(message);
    }

    public static UnbanErrorMessage AssignmentNotFound()
    {
        string message = $"{Title} \n Assignment was not found";
        return new UnbanErrorMessage(message);
    }

    public static UnbanErrorMessage StudentNotFound()
    {
        string message = $"{Title} \n Current repository is not attached to any student";
        return new UnbanErrorMessage(message);
    }

    public static UnbanErrorMessage Unauthorized()
    {
        string message = $"{Title} \n You are not authorized to create submission at this repository";
        return new UnbanErrorMessage(message);
    }

    public static UnbanErrorMessage InvalidMove(string sourceState)
    {
        string message = $"{Title} \n Cannot unban submission in {sourceState} state";
        return new UnbanErrorMessage(message);
    }

    public static UnbanErrorMessage Unexpected()
    {
        string message = $"{Title} \n Operation produces unexpected result";
        return new UnbanErrorMessage(message);
    }

    protected override string Message { get; }
}