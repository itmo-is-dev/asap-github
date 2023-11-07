using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionAcceptedResult
{
    private SubmissionAcceptedResult() { }

    public sealed record Success(SubmissionRateDto SubmissionRate) : SubmissionAcceptedResult;

    public sealed record InvalidState(SubmissionState SubmissionState) : SubmissionAcceptedResult;
}