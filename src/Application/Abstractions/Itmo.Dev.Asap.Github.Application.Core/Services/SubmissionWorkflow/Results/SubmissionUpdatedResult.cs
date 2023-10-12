using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionUpdatedResult
{
    private SubmissionUpdatedResult() { }

    public sealed record Success(SubmissionRateDto SubmissionRate, bool IsCreated) : SubmissionUpdatedResult;
}