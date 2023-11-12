namespace Itmo.Dev.Asap.Github.Application.Models.Assignments;

public record GithubAssignment(Guid Id, Guid SubjectCourseId, string BranchName);