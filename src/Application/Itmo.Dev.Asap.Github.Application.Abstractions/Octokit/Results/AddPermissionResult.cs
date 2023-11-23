namespace Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Results;

public enum AddPermissionResult
{
    Invited,
    ReInvited,
    Pending,
    AlreadyCollaborator,
    Failed,
}