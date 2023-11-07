namespace Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;

public enum AddPermissionResult
{
    Invited,
    ReInvited,
    Pending,
    AlreadyCollaborator,
    Failed,
}