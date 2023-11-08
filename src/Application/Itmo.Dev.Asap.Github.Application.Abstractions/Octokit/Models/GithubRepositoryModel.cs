namespace Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;

public record GithubRepositoryModel(long Id, string Name, bool IsTemplate);