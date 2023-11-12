using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow.Results;

public abstract record SubmissionNotAcceptedResult
{
    private SubmissionNotAcceptedResult() { }

    public sealed record Success(SubmissionRateDto SubmissionRate) : SubmissionNotAcceptedResult;

    public sealed record InvalidState(SubmissionState SubmissionState) : SubmissionNotAcceptedResult;
}