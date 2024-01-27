using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.ErrorMessages;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Models;

public record SubmissionCommandResult
{
    private SubmissionCommandResult() { }

    public sealed record Success : SubmissionCommandResult;

    public sealed record Failure(IErrorMessage ErrorMessage) : SubmissionCommandResult;
}