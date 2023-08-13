using Itmo.Dev.Asap.Github.Application.Core.Exceptions;
using Itmo.Dev.Asap.Github.Application.Core.Services;
using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Core.Dummy;

public class DummySubmissionService : ISubmissionService
{
    public Task<SubmissionDto> ActivateSubmissionAsync(Guid issuerId, Guid submissionId, CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionDto> BanSubmissionAsync(
        Guid issuerId,
        Guid studentId,
        Guid assignmentId,
        int? code,
        CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionDto> CreateSubmissionAsync(
        Guid issuerId,
        Guid userId,
        Guid assignmentId,
        string payload,
        CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionDto> DeactivateSubmissionAsync(Guid issuerId, Guid submissionId, CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionDto> DeleteSubmissionAsync(Guid issuerId, Guid submissionId, CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionDto> MarkSubmissionAsReviewedAsync(Guid issuerId, Guid submissionId, CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionRateDto> RateSubmissionAsync(
        Guid issuerId,
        Guid submissionId,
        double ratingPercent,
        double? extraPoints,
        CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionRateDto> UpdateSubmissionAsync(
        Guid issuerId,
        Guid userId,
        Guid assignmentId,
        int? code,
        DateOnly? dateTime,
        double? ratingPercent,
        double? extraPoints,
        CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }
}