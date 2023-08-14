using Itmo.Dev.Asap.Core.SubmissionWorkflow;
using Itmo.Dev.Asap.Github.Application.Core.Models;
using Itmo.Dev.Asap.Github.Application.Core.Services;
using Itmo.Dev.Asap.Github.Integrations.Core.Mapping;

namespace Itmo.Dev.Asap.Github.Integrations.Core.Services;

public class SubmissionWorkflowService : ISubmissionWorkflowService
{
    private readonly Asap.Core.SubmissionWorkflow.SubmissionWorkflowService.SubmissionWorkflowServiceClient _client;

    public SubmissionWorkflowService(
        Asap.Core.SubmissionWorkflow.SubmissionWorkflowService.SubmissionWorkflowServiceClient client)
    {
        _client = client;
    }

    public async Task<SubmissionActionMessageDto> SubmissionApprovedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var request = new ApprovedRequest
        {
            IssuerId = issuerId.ToString(),
            SubmissionId = submissionId.ToString(),
        };

        ApprovedResponse response = await _client.ApprovedAsync(request, cancellationToken: cancellationToken);

        return new SubmissionActionMessageDto(response.Message);
    }

    public async Task<SubmissionActionMessageDto> SubmissionReactivatedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var request = new ReactivatedRequest
        {
            IssuerId = issuerId.ToString(),
            SubmissionId = submissionId.ToString(),
        };

        ReactivatedResponse response = await _client.ReactivatedAsync(request, cancellationToken: cancellationToken);

        return new SubmissionActionMessageDto(response.Message);
    }

    public async Task<SubmissionUpdateResult> SubmissionUpdatedAsync(
        Guid issuerId,
        Guid userId,
        Guid assignmentId,
        string payload,
        CancellationToken cancellationToken)
    {
        var request = new UpdatedRequest
        {
            IssuerId = issuerId.ToString(),
            UserId = userId.ToString(),
            AssignmentId = assignmentId.ToString(),
            Payload = payload,
        };

        UpdatedResponse response = await _client.UpdatedAsync(request, cancellationToken: cancellationToken);

        return new SubmissionUpdateResult(response.Submission.ToDto(), response.IsCreated);
    }

    public async Task<SubmissionActionMessageDto> SubmissionAcceptedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var request = new AcceptedRequest
        {
            IssuerId = issuerId.ToString(),
            SubmissionId = submissionId.ToString(),
        };

        AcceptedResponse response = await _client.AcceptedAsync(request, cancellationToken: cancellationToken);

        return new SubmissionActionMessageDto(response.Message);
    }

    public async Task<SubmissionActionMessageDto> SubmissionRejectedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var request = new RejectedRequest
        {
            IssuerId = issuerId.ToString(),
            SubmissionId = submissionId.ToString(),
        };

        RejectedResponse response = await _client.RejectedAsync(request, cancellationToken: cancellationToken);

        return new SubmissionActionMessageDto(response.Message);
    }

    public async Task<SubmissionActionMessageDto> SubmissionAbandonedAsync(
        Guid issuerId,
        Guid submissionId,
        bool isTerminal,
        CancellationToken cancellationToken)
    {
        var request = new AbandonedRequest
        {
            IssuerId = issuerId.ToString(),
            SubmissionId = submissionId.ToString(),
            IsTerminal = isTerminal,
        };

        AbandonedResponse response = await _client.AbandonedAsync(request, cancellationToken: cancellationToken);

        return new SubmissionActionMessageDto(response.Message);
    }

    public async Task<SubmissionActionMessageDto> SubmissionNotAcceptedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var request = new NotAcceptedRequest
        {
            IssuerId = issuerId.ToString(),
            SubmissionId = submissionId.ToString(),
        };

        NotAcceptedResponse response = await _client.NotAcceptedAsync(request, cancellationToken: cancellationToken);

        return new SubmissionActionMessageDto(response.Message);
    }
}