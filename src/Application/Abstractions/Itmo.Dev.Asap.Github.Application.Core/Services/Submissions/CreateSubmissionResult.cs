using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.Submissions;

public abstract record CreateSubmissionResult
{
    private CreateSubmissionResult() { }

    public sealed record Success(SubmissionDto Submission) : CreateSubmissionResult;

    public sealed record Unauthorized : CreateSubmissionResult;

    public sealed record Unexpected : CreateSubmissionResult;
}