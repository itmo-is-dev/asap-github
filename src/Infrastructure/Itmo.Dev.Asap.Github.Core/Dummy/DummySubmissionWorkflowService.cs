using Itmo.Dev.Asap.Github.Application.Core.Exceptions;
using Itmo.Dev.Asap.Github.Application.Core.Models;
using Itmo.Dev.Asap.Github.Application.Core.Services;

namespace Itmo.Dev.Asap.Github.Core.Dummy;

public class DummySubmissionWorkflowService : ISubmissionWorkflowService
{
    public Task<SubmissionActionMessageDto> SubmissionApprovedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionActionMessageDto> SubmissionReactivatedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionUpdateResult> SubmissionUpdatedAsync(
        Guid issuerId,
        Guid userId,
        Guid assignmentId,
        string payload,
        CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionActionMessageDto> SubmissionAcceptedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionActionMessageDto> SubmissionRejectedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionActionMessageDto> SubmissionAbandonedAsync(
        Guid issuerId,
        Guid submissionId,
        bool isTerminal,
        CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<SubmissionActionMessageDto> SubmissionNotAcceptedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }
}