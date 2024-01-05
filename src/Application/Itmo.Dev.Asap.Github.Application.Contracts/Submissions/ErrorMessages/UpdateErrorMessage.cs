namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class UpdateErrorMessage : ErrorMessage
{
    private const string Title = "Error occured while processing Update command";

    private UpdateErrorMessage(string message)
    {
        Message = message;
    }

    public static UpdateErrorMessage IssuerNotFound()
    {
        string message = $"{Title} \n Issuer was not found";
        return new UpdateErrorMessage(message);
    }

    public static UpdateErrorMessage AssignmentNotFound()
    {
        string message = $"{Title} \n Assignment was not found";
        return new UpdateErrorMessage(message);
    }

    public static UpdateErrorMessage StudentNotFound()
    {
        string message = $"{Title} \n Current repository is not attached to any student";
        return new UpdateErrorMessage(message);
    }

    public static UpdateErrorMessage WithMessage(string m)
    {
        string message = $"{Title} \n {m}";
        return new UpdateErrorMessage(message);
    }

    public static UpdateErrorMessage Unexpected()
    {
        string message = $"{Title} \n Failed to rate submission";
        return new UpdateErrorMessage(message);
    }

    protected override string Message { get; }
}