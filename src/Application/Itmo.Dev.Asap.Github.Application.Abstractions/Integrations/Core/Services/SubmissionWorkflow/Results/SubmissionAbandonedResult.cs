using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionAbandonedResult
{
    private SubmissionAbandonedResult() { }

    public sealed record Success(int SubmissionCode) : SubmissionAbandonedResult;

    public sealed record InvalidState(SubmissionState SubmissionState) : SubmissionAbandonedResult;
}