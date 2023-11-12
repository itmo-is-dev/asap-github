using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionReactivatedResult
{
    private SubmissionReactivatedResult() { }

    public sealed record Success : SubmissionReactivatedResult;

    public sealed record InvalidState(SubmissionState SubmissionState) : SubmissionReactivatedResult;
}