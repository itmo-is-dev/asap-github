using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionNotAcceptedResult
{
    private SubmissionNotAcceptedResult() { }

    public sealed record Success(SubmissionRateDto SubmissionRate) : SubmissionNotAcceptedResult;

    public sealed record InvalidState(SubmissionStateDto SubmissionState) : SubmissionNotAcceptedResult;
}