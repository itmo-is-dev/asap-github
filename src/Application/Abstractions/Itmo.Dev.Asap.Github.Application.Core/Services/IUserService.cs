using Itmo.Dev.Asap.Github.Application.Dto.Users;

namespace Itmo.Dev.Asap.Github.Application.Core.Services;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(
        string firstName,
        string middleName,
        string lastName,
        CancellationToken cancellationToken);

    Task<UserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
}