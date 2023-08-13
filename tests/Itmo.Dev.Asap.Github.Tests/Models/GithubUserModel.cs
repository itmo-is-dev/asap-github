using Itmo.Dev.Asap.Github.Domain.Users;

namespace Itmo.Dev.Asap.Github.Tests.Models;

public record GithubUserModel(Guid Id, string Username)
{
    public GithubUser ToEntity()
    {
        return new GithubUser(Id, Username);
    }
}