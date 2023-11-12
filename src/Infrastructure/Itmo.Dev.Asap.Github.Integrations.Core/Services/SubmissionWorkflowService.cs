using Itmo.Dev.Asap.Core.SubmissionWorkflow;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow.Results;
using Itmo.Dev.Asap.Github.Common.Exceptions;
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

    public async Task<SubmissionApprovedResult> SubmissionApprovedAsync(
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

        return response.ResultCase switch
        {
            ApprovedResponse.ResultOneofCase.Success
                => new SubmissionApprovedResult.Success(response.Success.SubmissionRate.ToDto()),

            ApprovedResponse.ResultOneofCase.InvalidState
                => new SubmissionApprovedResult.InvalidState(response.InvalidState.SubmissionState.ToDto()),

            ApprovedResponse.ResultOneofCase.None or _ => throw new UnexpectedOperationResultException(),
        };
    }

    public async Task<SubmissionReactivatedResult> SubmissionReactivatedAsync(
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

        return response.ResultCase switch
        {
            ReactivatedResponse.ResultOneofCase.Success => new SubmissionReactivatedResult.Success(),

            ReactivatedResponse.ResultOneofCase.InvalidState
                => new SubmissionReactivatedResult.InvalidState(response.InvalidState.SubmissionState.ToDto()),

            ReactivatedResponse.ResultOneofCase.None or _ => throw new UnexpectedOperationResultException(),
        };
    }

    public async Task<SubmissionUpdatedResult> SubmissionUpdatedAsync(
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

        return response.ResultCase switch
        {
            UpdatedResponse.ResultOneofCase.Success => new SubmissionUpdatedResult.Success(
                response.Success.SubmissionRate.ToDto(),
                response.Success.IsCreated),

            UpdatedResponse.ResultOneofCase.None or _ => throw new UnexpectedOperationResultException(),
        };
    }

    public async Task<SubmissionAcceptedResult> SubmissionAcceptedAsync(
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

        return response.ResultCase switch
        {
            AcceptedResponse.ResultOneofCase.Success
                => new SubmissionAcceptedResult.Success(response.Success.SubmissionRate.ToDto()),

            AcceptedResponse.ResultOneofCase.InvalidState
                => new SubmissionAcceptedResult.InvalidState(response.InvalidState.SubmissionState.ToDto()),

            AcceptedResponse.ResultOneofCase.None or _ => throw new UnexpectedOperationResultException(),
        };
    }

    public async Task<SubmissionRejectedResult> SubmissionRejectedAsync(
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

        return response.ResultCase switch
        {
            RejectedResponse.ResultOneofCase.Success
                => new SubmissionRejectedResult.Success(response.Success.SubmissionCode),

            RejectedResponse.ResultOneofCase.InvalidState
                => new SubmissionRejectedResult.InvalidState(response.InvalidState.SubmissionState.ToDto()),

            RejectedResponse.ResultOneofCase.None or _ => throw new UnexpectedOperationResultException(),
        };
    }

    public async Task<SubmissionAbandonedResult> SubmissionAbandonedAsync(
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

        return response.ResultCase switch
        {
            AbandonedResponse.ResultOneofCase.Success
                => new SubmissionAbandonedResult.Success(response.Success.SubmissionCode),

            AbandonedResponse.ResultOneofCase.InvalidState
                => new SubmissionAbandonedResult.InvalidState(response.InvalidState.SubmissionState.ToDto()),

            AbandonedResponse.ResultOneofCase.None or _ => throw new UnexpectedOperationResultException(),
        };
    }

    public async Task<SubmissionNotAcceptedResult> SubmissionNotAcceptedAsync(
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

        return response.ResultCase switch
        {
            NotAcceptedResponse.ResultOneofCase.Success
                => new SubmissionNotAcceptedResult.Success(response.Success.SubmissionRate.ToDto()),

            NotAcceptedResponse.ResultOneofCase.InvalidState
                => new SubmissionNotAcceptedResult.InvalidState(response.InvalidState.SubmissionState.ToDto()),

            NotAcceptedResponse.ResultOneofCase.None or _ => throw new UnexpectedOperationResultException(),
        };
    }
}