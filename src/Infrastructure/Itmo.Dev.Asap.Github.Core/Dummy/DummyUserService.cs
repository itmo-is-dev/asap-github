using Itmo.Dev.Asap.Github.Application.Core.Exceptions;
using Itmo.Dev.Asap.Github.Application.Core.Services;
using Itmo.Dev.Asap.Github.Application.Dto.Users;

namespace Itmo.Dev.Asap.Github.Core.Dummy;

public class DummyUserService : IUserService
{
    public Task<UserDto> CreateUserAsync(string firstName, string middleName, string lastName, CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }

    public Task<UserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        throw new AsapCoreException("Core integration disabled");
    }
}