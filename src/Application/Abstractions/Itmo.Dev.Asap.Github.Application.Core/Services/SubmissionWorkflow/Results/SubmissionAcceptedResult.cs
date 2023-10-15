using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionAcceptedResult
{
    private SubmissionAcceptedResult() { }

    public sealed record Success(SubmissionRateDto SubmissionRate) : SubmissionAcceptedResult;

    public sealed record InvalidState(SubmissionStateDto SubmissionState) : SubmissionAcceptedResult;
}