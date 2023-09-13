namespace Itmo.Dev.Asap.Github.Application.Core.Services.SubjectCourses;

public interface ISubjectCourseService
{
    Task UpdateMentorsAsync(
        Guid subjectCourseId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);

    Task<GetSubjectCourseStudentsResponse> GetSubjectCourseStudents(
        GetSubjectCourseStudentsRequest request,
        CancellationToken cancellationToken);
}