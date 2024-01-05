namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class BanErrorMessage : ErrorMessage
{
    private const string Title = "Error occured while processing Ban command";

    private BanErrorMessage(string message)
    {
        Message = message;
    }

    public static BanErrorMessage IssuerNotFound()
    {
        string message = $"{Title} \n Issuer was not found";
        return new BanErrorMessage(message);
    }

    public static BanErrorMessage AssignmentNotFound()
    {
        string message = $"{Title} \n Assignment was not found";
        return new BanErrorMessage(message);
    }

    public static BanErrorMessage StudentNotFound()
    {
        string message = $"{Title} \n Current repository is not attached to any student";
        return new BanErrorMessage(message);
    }

    protected override string Message { get; }
}
