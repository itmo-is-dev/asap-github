using Itmo.Dev.Asap.Github.Domain.Assignments;

namespace Itmo.Dev.Asap.Github.Tests.Models;

public record GithubAssignmentModel(Guid Id, Guid SubjectCourseId, string BranchName)
{
    public GithubAssignment ToEntity()
    {
        return new GithubAssignment(Id, SubjectCourseId, BranchName);
    }
}