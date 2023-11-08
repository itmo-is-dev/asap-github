namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Models;

public record SubmissionCommandResult
{
    private SubmissionCommandResult() { }

    public sealed record Success : SubmissionCommandResult;

    public sealed record Failure(string Message) : SubmissionCommandResult;
}