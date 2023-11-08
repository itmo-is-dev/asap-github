using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.Submissions;

public abstract record RateSubmissionResult
{
    private RateSubmissionResult() { }

    public sealed record Success(SubmissionRateDto Submission) : RateSubmissionResult;

    public sealed record Failure(string Message) : RateSubmissionResult;
}