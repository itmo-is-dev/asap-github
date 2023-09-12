using Itmo.Dev.Asap.Github.Application.Dto.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.Submissions;

public interface ISubmissionService
{
    Task<SubmissionDto> ActivateSubmissionAsync(Guid issuerId, Guid submissionId, CancellationToken cancellationToken);

    Task<SubmissionDto> BanSubmissionAsync(
        Guid issuerId,
        Guid studentId,
        Guid assignmentId,
        int? code,
        CancellationToken cancellationToken);

    Task<SubmissionDto> CreateSubmissionAsync(
        Guid issuerId,
        Guid userId,
        Guid assignmentId,
        string payload,
        CancellationToken cancellationToken);

    Task<SubmissionDto> DeactivateSubmissionAsync(Guid issuerId, Guid submissionId, CancellationToken cancellationToken);

    Task<SubmissionDto> DeleteSubmissionAsync(Guid issuerId, Guid submissionId, CancellationToken cancellationToken);

    Task<SubmissionDto> MarkSubmissionAsReviewedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken);

    Task<RateSubmissionResult> RateSubmissionAsync(
        Guid issuerId,
        Guid submissionId,
        double ratingPercent,
        double? extraPoints,
        CancellationToken cancellationToken);

    Task<UpdateSubmissionResult> UpdateSubmissionAsync(
        Guid issuerId,
        Guid userId,
        Guid assignmentId,
        int? code,
        DateOnly? dateTime,
        double? ratingPercent,
        double? extraPoints,
        CancellationToken cancellationToken);
}