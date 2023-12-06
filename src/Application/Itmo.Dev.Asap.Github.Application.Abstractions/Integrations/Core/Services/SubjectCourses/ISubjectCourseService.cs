namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubjectCourses;

public interface ISubjectCourseService
{
    Task UpdateMentorsAsync(
        Guid subjectCourseId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);

    Task<GetSubjectCourseStudentsResponse> GetSubjectCourseStudents(
        GetSubjectCourseStudentsRequest request,
        CancellationToken cancellationToken);

    Task<IEnumerable<Guid>> GetSubjectCourseMentors(Guid subjectCourseId, CancellationToken cancellationToken);
}