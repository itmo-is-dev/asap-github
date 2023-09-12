using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.Submissions;

public abstract record UpdateSubmissionResult
{
    private UpdateSubmissionResult() { }

    public sealed record Success(SubmissionRateDto Submission) : UpdateSubmissionResult;

    public sealed record Failure(string ErrorMessage) : UpdateSubmissionResult;
}