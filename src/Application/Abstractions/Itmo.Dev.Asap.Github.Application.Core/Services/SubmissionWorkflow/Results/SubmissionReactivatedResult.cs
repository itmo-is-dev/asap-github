using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionReactivatedResult
{
    private SubmissionReactivatedResult() { }

    public sealed record Success : SubmissionReactivatedResult;

    public sealed record InvalidState(SubmissionStateDto SubmissionState) : SubmissionReactivatedResult;
}