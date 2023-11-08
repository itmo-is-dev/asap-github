using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.Submissions;

public abstract record UnbanSubmissionResult
{
    private UnbanSubmissionResult() { }

    public sealed record Success(SubmissionDto Submission) : UnbanSubmissionResult;

    public sealed record Unauthorized : UnbanSubmissionResult;

    public sealed record InvalidMove(SubmissionState SourceState) : UnbanSubmissionResult;
}