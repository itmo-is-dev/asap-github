namespace Itmo.Dev.Asap.Github.Application.Core.Services;

public interface IPermissionService
{
    Task<bool> IsSubmissionMentorAsync(Guid userId, Guid submissionId, CancellationToken cancellationToken);
}