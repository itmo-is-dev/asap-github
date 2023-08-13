using Itmo.Dev.Asap.Github.Application.Core.Services;

namespace Itmo.Dev.Asap.Github.Core.Dummy;

public class DummySubjectCourseService : ISubjectCourseService
{
    public Task UpdateMentorsAsync(Guid subjectCourseId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Guid>> GetSubjectCourseStudentIds(Guid subjectCourseId, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<Guid>>(Array.Empty<Guid>());
    }
}