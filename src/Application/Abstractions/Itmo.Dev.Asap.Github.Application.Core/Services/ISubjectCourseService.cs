namespace Itmo.Dev.Asap.Github.Application.Core.Services;

public interface ISubjectCourseService
{
    Task UpdateMentorsAsync(
        Guid subjectCourseId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Guid>> GetSubjectCourseStudentIds(
        Guid subjectCourseId,
        CancellationToken cancellationToken);
}