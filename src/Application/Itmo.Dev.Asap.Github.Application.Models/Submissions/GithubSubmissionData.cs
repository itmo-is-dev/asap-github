namespace Itmo.Dev.Asap.Github.Application.Models.Submissions;

public record GithubSubmissionData(Guid SubmissionId, Guid UserId, Guid AssignmentId, long TaskId, string FileLink);