using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.Submissions;

public abstract record RateSubmissionResult
{
    private RateSubmissionResult() { }

    public sealed record Success(SubmissionRateDto Submission) : RateSubmissionResult;

    public sealed record Failure(string Message) : RateSubmissionResult;
}