using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Models;

public abstract record CreateSubmissionDataResult
{
    private CreateSubmissionDataResult() { }

    public sealed record Success(GithubSubmissionData Data) : CreateSubmissionDataResult;

    public sealed record Failure(SubjectCourseDumpError Error) : CreateSubmissionDataResult;
}