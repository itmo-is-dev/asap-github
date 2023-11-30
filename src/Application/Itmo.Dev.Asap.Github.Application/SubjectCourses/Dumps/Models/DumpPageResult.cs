using Itmo.Dev.Asap.Github.Application.Models.Submissions;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Models;

public abstract record DumpPageResult
{
    private DumpPageResult() { }

    public sealed record Success(IReadOnlyCollection<GithubSubmissionData> Data) : DumpPageResult;

    public sealed record Failure(SubjectCourseDumpError Error) : DumpPageResult;
}