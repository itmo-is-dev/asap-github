using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.Submissions;

public abstract record CreateSubmissionResult
{
    private CreateSubmissionResult() { }

    public sealed record Success(SubmissionDto Submission) : CreateSubmissionResult;

    public sealed record Unauthorized : CreateSubmissionResult;

    public sealed record Unexpected : CreateSubmissionResult;
}