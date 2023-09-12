using Google.Protobuf.WellKnownTypes;
using Itmo.Dev.Asap.Core.Submissions;
using Itmo.Dev.Asap.Github.Application.Core.Services.Submissions;
using Itmo.Dev.Asap.Github.Application.Dto.Submissions;
using Itmo.Dev.Asap.Github.Integrations.Core.Mapping;

namespace Itmo.Dev.Asap.Github.Integrations.Core.Services;

public class SubmissionService : ISubmissionService
{
    private readonly Asap.Core.Submissions.SubmissionService.SubmissionServiceClient _client;

    public SubmissionService(Asap.Core.Submissions.SubmissionService.SubmissionServiceClient client)
    {
        _client = client;
    }

    public async Task<SubmissionDto> ActivateSubmissionAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var request = new ActivateRequest
        {
            IssuerId = issuerId.ToString(),
            SubmissionId = submissionId.ToString(),
        };

        ActivateResponse response = await _client.ActivateAsync(request, cancellationToken: cancellationToken);

        return response.Submission.ToDto();
    }

    public async Task<SubmissionDto> BanSubmissionAsync(
        Guid issuerId,
        Guid studentId,
        Guid assignmentId,
        int? code,
        CancellationToken cancellationToken)
    {
        var request = new BanRequest
        {
            IssuerId = issuerId.ToString(),
            StudentId = studentId.ToString(),
            AssignmentId = assignmentId.ToString(),
            Code = code,
        };

        BanResponse response = await _client.BanAsync(request, cancellationToken: cancellationToken);

        return response.Submission.ToDto();
    }

    public async Task<SubmissionDto> CreateSubmissionAsync(
        Guid issuerId,
        Guid userId,
        Guid assignmentId,
        string payload,
        CancellationToken cancellationToken)
    {
        var request = new CreateRequest
        {
            IssuerId = issuerId.ToString(),
            StudentId = userId.ToString(),
            AssignmentId = assignmentId.ToString(),
            Payload = payload,
        };

        CreateResponse response = await _client.CreateAsync(request, cancellationToken: cancellationToken);

        return response.Submission.ToDto();
    }

    public async Task<SubmissionDto> DeactivateSubmissionAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var request = new DeactivateRequest
        {
            IssuerId = issuerId.ToString(),
            SubmissionId = submissionId.ToString(),
        };

        DeactivateResponse response = await _client.DeactivateAsync(request, cancellationToken: cancellationToken);

        return response.Submission.ToDto();
    }

    public async Task<SubmissionDto> DeleteSubmissionAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var request = new DeleteRequest
        {
            IssuerId = issuerId.ToString(),
            SubmissionId = submissionId.ToString(),
        };

        DeleteResponse response = await _client.DeleteAsync(request, cancellationToken: cancellationToken);

        return response.Submission.ToDto();
    }

    public async Task<SubmissionDto> MarkSubmissionAsReviewedAsync(
        Guid issuerId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var request = new MarkReviewedRequest
        {
            IssuerId = issuerId.ToString(),
            SubmissionId = submissionId.ToString(),
        };

        MarkReviewedResponse response = await _client.MarkReviewedAsync(request, cancellationToken: cancellationToken);

        return response.Submission.ToDto();
    }

    public async Task<RateSubmissionResult> RateSubmissionAsync(
        Guid issuerId,
        Guid submissionId,
        double ratingPercent,
        double? extraPoints,
        CancellationToken cancellationToken)
    {
        var request = new RateRequest
        {
            IssuerId = issuerId.ToString(),
            SubmissionId = submissionId.ToString(),
            RatingPercent = ratingPercent,
            ExtraPoints = extraPoints,
        };

        RateResponse response = await _client.RateAsync(request, cancellationToken: cancellationToken);

        return response.ResultCase switch
        {
            RateResponse.ResultOneofCase.Submission => new RateSubmissionResult.Success(response.Submission.ToDto()),
            RateResponse.ResultOneofCase.ErrorMessage => new RateSubmissionResult.Failure(response.ErrorMessage),
            _ or RateResponse.ResultOneofCase.None => new RateSubmissionResult.Failure("Failed to rate submission"),
        };
    }

    public async Task<SubmissionRateDto> UpdateSubmissionAsync(
        Guid issuerId,
        Guid userId,
        Guid assignmentId,
        int? code,
        DateOnly? dateTime,
        double? ratingPercent,
        double? extraPoints,
        CancellationToken cancellationToken)
    {
        var request = new UpdateRequest
        {
            IssuerId = issuerId.ToString(),
            UserId = userId.ToString(),
            AssignmentId = assignmentId.ToString(),
            Code = code,
            RatingPercent = ratingPercent,
            ExtraPoints = extraPoints,
        };

        if (dateTime is not null)
        {
            var dateOnly = dateTime.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.Zero));
            request.SubmissionDateValue = Timestamp.FromDateTime(DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc));
        }

        UpdateResponse response = await _client.UpdateAsync(request, cancellationToken: cancellationToken);

        return response.Submission.ToDto();
    }
}