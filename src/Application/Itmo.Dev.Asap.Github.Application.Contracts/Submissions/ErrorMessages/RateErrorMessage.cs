namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class RateErrorMessage : ErrorMessage
{
    private const string Title = "Error occured while processing Rate command";

    private RateErrorMessage(string message)
    {
        Message = message;
    }

    public static RateErrorMessage IssuerNotFound()
    {
        string message = $"{Title} \n Issuer was not found";
        return new RateErrorMessage(message);
    }

    public static RateErrorMessage SubmissionNotFound()
    {
        string message = $"{Title} \n Submission was not found";
        return new RateErrorMessage(message);
    }

    public static RateErrorMessage WithMessage(string m)
    {
        string message = $"{Title} \n {m}";
        return new RateErrorMessage(message);
    }

    public static RateErrorMessage Unexpected()
    {
        string message = $"{Title} \n Failed to rate submission";
        return new RateErrorMessage(message);
    }

    protected override string Message { get; }
}