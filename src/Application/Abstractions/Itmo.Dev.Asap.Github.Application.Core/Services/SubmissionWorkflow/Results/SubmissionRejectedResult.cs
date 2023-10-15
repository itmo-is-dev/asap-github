using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionRejectedResult
{
    private SubmissionRejectedResult() { }

    public sealed record Success(int SubmissionCode) : SubmissionRejectedResult;

    public sealed record InvalidState(SubmissionStateDto SubmissionState) : SubmissionRejectedResult;
}