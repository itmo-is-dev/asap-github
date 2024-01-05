namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

public class CreateSubmissionErrorMessage : ErrorMessage
{
    private const string Title = "Error occured while processing Create Submission command";

    private CreateSubmissionErrorMessage(string message)
    {
        Message = message;
    }

    public static CreateSubmissionErrorMessage IssuerNotFound()
    {
        string message = $"{Title} \n Issuer was not found";
        return new CreateSubmissionErrorMessage(message);
    }

    public static CreateSubmissionErrorMessage AssignmentNotFound()
    {
        string message = $"{Title} \n Assignment was not found";
        return new CreateSubmissionErrorMessage(message);
    }

    public static CreateSubmissionErrorMessage StudentNotFound()
    {
        string message = $"{Title} \n Current repository is not attached to any student";
        return new CreateSubmissionErrorMessage(message);
    }

    public static CreateSubmissionErrorMessage Unauthorized()
    {
        string message = $"{Title} \n You are not authorized to create submission at this repository";
        return new CreateSubmissionErrorMessage(message);
    }

    public static CreateSubmissionErrorMessage Unexpected()
    {
        string message = $"{Title} \n Operation produces unexpected result";
        return new CreateSubmissionErrorMessage(message);
    }

    protected override string Message { get; }
}
