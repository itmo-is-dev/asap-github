using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionApprovedResult
{
    private SubmissionApprovedResult() { }

    public sealed record Success(SubmissionRateDto SubmissionRate) : SubmissionApprovedResult;

    public sealed record InvalidState(SubmissionState State) : SubmissionApprovedResult;
}