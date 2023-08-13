namespace Itmo.Dev.Asap.Github.Application.Octokit.Models;

public enum AddPermissionResult
{
    Invited,
    ReInvited,
    Pending,
    AlreadyCollaborator,
}