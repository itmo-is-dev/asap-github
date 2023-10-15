using Itmo.Dev.Asap.Github.Application.Core.Services.SubmissionWorkflow.Results;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.SubmissionWorkflow;

public interface ISubmissionWorkflowService
{
    Task<SubmissionApprovedResult> SubmissionApprovedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken);

    Task<SubmissionReactivatedResult> SubmissionReactivatedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken);

    Task<SubmissionUpdatedResult> SubmissionUpdatedAsync(
        Guid issuerId,
        Guid userId,
        Guid assignmentId,
        string payload,
        CancellationToken cancellationToken);

    Task<SubmissionAcceptedResult> SubmissionAcceptedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken);

    Task<SubmissionRejectedResult> SubmissionRejectedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken);

    Task<SubmissionAbandonedResult> SubmissionAbandonedAsync(
        Guid issuerId,
        Guid submissionId,
        bool isTerminal,
        CancellationToken cancellationToken);

    Task<SubmissionNotAcceptedResult> SubmissionNotAcceptedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken);
}