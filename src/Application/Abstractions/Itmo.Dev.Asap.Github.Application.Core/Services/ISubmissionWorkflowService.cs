using Itmo.Dev.Asap.Github.Application.Core.Models;

namespace Itmo.Dev.Asap.Github.Application.Core.Services;

public interface ISubmissionWorkflowService
{
    Task<SubmissionActionMessageDto> SubmissionApprovedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken);

    Task<SubmissionActionMessageDto> SubmissionReactivatedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken);

    Task<SubmissionUpdateResult> SubmissionUpdatedAsync(
        Guid issuerId,
        Guid userId,
        Guid assignmentId,
        string payload,
        CancellationToken cancellationToken);

    Task<SubmissionActionMessageDto> SubmissionAcceptedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken);

    Task<SubmissionActionMessageDto> SubmissionRejectedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken);

    Task<SubmissionActionMessageDto> SubmissionAbandonedAsync(
        Guid issuerId,
        Guid submissionId,
        bool isTerminal,
        CancellationToken cancellationToken);

    Task<SubmissionActionMessageDto> SubmissionNotAcceptedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken);
}