using Itmo.Dev.Asap.Core.Permissions;
using Itmo.Dev.Asap.Github.Application.Core.Services;

namespace Itmo.Dev.Asap.Github.Integrations.Core.Services;

public class PermissionService : IPermissionService
{
    private readonly Asap.Core.Permissions.PermissionService.PermissionServiceClient _client;

    public PermissionService(Asap.Core.Permissions.PermissionService.PermissionServiceClient client)
    {
        _client = client;
    }

    public async Task<bool> IsSubmissionMentorAsync(Guid userId, Guid submissionId, CancellationToken cancellationToken)
    {
        var request = new IsSubmissionMentorRequest
        {
            UserId = userId.ToString(),
            SubmissionId = submissionId.ToString(),
        };

        IsSubmissionMentorResponse response = await _client
            .IsSubmissionMentorAsync(request, cancellationToken: cancellationToken);

        return response.IsMentor;
    }
}