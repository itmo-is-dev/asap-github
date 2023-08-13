using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Models;

public record SubmissionUpdateResult(SubmissionRateDto Submission, bool IsCreated);