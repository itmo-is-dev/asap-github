using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionRejectedResult
{
    private SubmissionRejectedResult() { }

    public sealed record Success(int SubmissionCode) : SubmissionRejectedResult;

    public sealed record InvalidState(SubmissionState SubmissionState) : SubmissionRejectedResult;
}