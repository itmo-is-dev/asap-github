using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionUpdatedResult
{
    private SubmissionUpdatedResult() { }

    public sealed record Success(SubmissionRateDto SubmissionRate, bool IsCreated) : SubmissionUpdatedResult;
}