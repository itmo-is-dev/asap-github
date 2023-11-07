namespace Itmo.Dev.Asap.Github.Application.Models.Users;

public record EnrichedGithubUser(Guid Id, long GithubId, string GithubUsername);