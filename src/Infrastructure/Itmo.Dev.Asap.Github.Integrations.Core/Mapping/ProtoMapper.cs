using Itmo.Dev.Asap.Core;
using Itmo.Dev.Asap.Core.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using Itmo.Dev.Asap.Github.Common.Tools;
using SubmissionState = Itmo.Dev.Asap.Github.Application.Models.Submissions.SubmissionState;

namespace Itmo.Dev.Asap.Github.Integrations.Core.Mapping;

internal static class ProtoMapper
{
    public static UserDto ToDto(this User user)
        => new UserDto(user.Id.ToGuid(), user.FirstName, user.MiddleName, user.LastName, user.UniversityId);

    public static StudentDto ToDto(this Student student)
        => new StudentDto(student.User.ToDto(), student.GroupId?.ToGuid(), student.GroupName);

    public static SubmissionDto ToDto(this Submission submission)
    {
        return new SubmissionDto(
            submission.Id.ToGuid(),
            submission.Code,
            submission.SubmissionDate.ToDateTimeOffset(),
            submission.StudentId.ToGuid(),
            submission.AssignmentId.ToGuid(),
            submission.Payload,
            submission.ExtraPoints,
            submission.Points,
            submission.AssignmentShortName,
            submission.State.ToDto());
    }

    public static SubmissionRateDto ToDto(this SubmissionRate submissionRate)
    {
        return new SubmissionRateDto(
            submissionRate.SubmissionId.ToGuid(),
            submissionRate.Code,
            submissionRate.State,
            submissionRate.SubmissionDate.ToDateTimeOffset(),
            submissionRate.Rating,
            submissionRate.RawPoints,
            submissionRate.MaxRawPoints,
            submissionRate.ExtraPoints,
            submissionRate.PenaltyPoints,
            submissionRate.TotalPoints);
    }

    public static SubmissionState ToDto(this Asap.Core.Models.SubmissionState state)
    {
        return state switch
        {
            Asap.Core.Models.SubmissionState.Active => SubmissionState.Active,
            Asap.Core.Models.SubmissionState.Inactive => SubmissionState.Inactive,
            Asap.Core.Models.SubmissionState.Deleted => SubmissionState.Deleted,
            Asap.Core.Models.SubmissionState.Completed => SubmissionState.Completed,
            Asap.Core.Models.SubmissionState.Reviewed => SubmissionState.Reviewed,
            Asap.Core.Models.SubmissionState.Banned => SubmissionState.Banned,
            Asap.Core.Models.SubmissionState.None or _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
        };
    }
}