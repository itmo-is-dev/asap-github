using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.Submissions;

public interface ISubmissionService
{
    Task<SubmissionDto> ActivateSubmissionAsync(Guid issuerId, Guid submissionId, CancellationToken cancellationToken);

    Task<SubmissionDto> BanSubmissionAsync(
        Guid issuerId,
        Guid studentId,
        Guid assignmentId,
        int? code,
        CancellationToken cancellationToken);

    Task<UnbanSubmissionResult> UnbanSubmissionAsync(
        Guid issuerId,
        Guid studentId,
        Guid assignmentId,
        int? code,
        CancellationToken cancellationToken);

    Task<CreateSubmissionResult> CreateSubmissionAsync(
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

    Task<QueryFirstSubmissionsResponse> QueryFirstCompletedSubmissions(
        Guid subjectCourseId,
        int pageSize,
        string? pageToken,
        CancellationToken cancellationToken);
}