using RichEntity.Annotations;

namespace Itmo.Dev.Asap.Github.Domain.Users;

public partial class GithubUser : IEntity<Guid>
{
    public GithubUser(Guid id, long githubId) : this(id)
    {
        GithubId = githubId;
    }

    public long GithubId { get; set; }
}