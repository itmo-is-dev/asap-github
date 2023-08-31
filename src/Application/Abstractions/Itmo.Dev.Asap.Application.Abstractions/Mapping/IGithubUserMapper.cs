using Itmo.Dev.Asap.Github.Application.Dto.Users;
using Itmo.Dev.Asap.Github.Domain.Users;

namespace Itmo.Dev.Asap.Application.Abstractions.Mapping;

public interface IGithubUserMapper
{
    Task<GithubUserDto> MapAsync(GithubUser user, CancellationToken cancellationToken);
}