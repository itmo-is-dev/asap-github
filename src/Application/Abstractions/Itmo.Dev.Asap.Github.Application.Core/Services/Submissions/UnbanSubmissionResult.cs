using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.Submissions;

public abstract record UnbanSubmissionResult
{
    private UnbanSubmissionResult() { }

    public sealed record Success(SubmissionDto Submission) : UnbanSubmissionResult;

    public sealed record Unauthorized : UnbanSubmissionResult;

    public sealed record InvalidMove(SubmissionStateDto SourceState) : UnbanSubmissionResult;
}