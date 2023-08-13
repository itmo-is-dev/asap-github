using RichEntity.Annotations;

namespace Itmo.Dev.Asap.Github.Domain.Users;

public partial class GithubUser : IEntity<Guid>
{
    public GithubUser(Guid id, string username) : this(id)
    {
        Username = username;
    }

    public string Username { get; set; }
}