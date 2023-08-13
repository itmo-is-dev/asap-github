using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Octokit;

namespace Itmo.Dev.Asap.Github.Application.Octokit.Services;

public interface IGithubRepositoryService
{
    Task AddTeamPermission(
        string organization,
        string repositoryName,
        Team team,
        Permission permission);

    Task CreateRepositoryFromTemplate(string organization, string newRepositoryName, string templateName);

    Task<AddPermissionResult> AddUserPermission(
        string organization,
        string repositoryName,
        string username,
        Permission permission);
}