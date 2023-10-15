using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionApprovedResult
{
    private SubmissionApprovedResult() { }

    public sealed record Success(SubmissionRateDto SubmissionRate) : SubmissionApprovedResult;

    public sealed record InvalidState(SubmissionStateDto State) : SubmissionApprovedResult;
}