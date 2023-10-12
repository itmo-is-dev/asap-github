using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionAbandonedResult
{
    private SubmissionAbandonedResult() { }

    public sealed record Success(int SubmissionCode) : SubmissionAbandonedResult;

    public sealed record InvalidState(SubmissionStateDto SubmissionState) : SubmissionAbandonedResult;
}