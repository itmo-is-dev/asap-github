using Itmo.Dev.Asap.Github.Application.Contracts.Users.Queries;
using Itmo.Dev.Asap.Github.Users;
using Riok.Mapperly.Abstractions;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;

[Mapper]
public static partial class GithubUserServiceMapper
{
    public static partial FindUsersByIds.Query MapTo(this FindByIdsRequest request);

    public static partial FindByIdsResponse MapFrom(this FindUsersByIds.Response response);
}