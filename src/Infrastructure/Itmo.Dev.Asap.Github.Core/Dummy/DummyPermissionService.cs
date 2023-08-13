using Itmo.Dev.Asap.Github.Application.Core.Services;

namespace Itmo.Dev.Asap.Github.Core.Dummy;

public class DummyPermissionService : IPermissionService
{
    public Task<bool> IsSubmissionMentorAsync(Guid userId, Guid submissionId, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}