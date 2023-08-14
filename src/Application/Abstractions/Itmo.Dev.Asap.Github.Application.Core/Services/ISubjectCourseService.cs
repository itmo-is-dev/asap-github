using Itmo.Dev.Asap.Github.Application.Core.Models;

namespace Itmo.Dev.Asap.Github.Application.Core.Services;

public interface ISubjectCourseService
{
    Task UpdateMentorsAsync(
        Guid subjectCourseId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);

    IAsyncEnumerable<StudentDto> GetSubjectCourseStudentIds(
        Guid subjectCourseId,
        CancellationToken cancellationToken);
}