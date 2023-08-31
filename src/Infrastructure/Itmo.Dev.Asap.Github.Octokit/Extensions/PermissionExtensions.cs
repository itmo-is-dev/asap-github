using Itmo.Dev.Asap.Github.Application.Octokit.Models;

namespace Itmo.Dev.Asap.Github.Octokit.Extensions;

internal static class PermissionExtensions
{
    public static string ToGithubApiString(this RepositoryPermission permission)
    {
        return permission switch
        {
            RepositoryPermission.Pull => "pull",
            RepositoryPermission.Triage => "triage",
            RepositoryPermission.Push => "push",
            RepositoryPermission.Maintain => "maintain",
            RepositoryPermission.Admin => "admin",
            _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, null),
        };
    }
}