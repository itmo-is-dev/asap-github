using Itmo.Dev.Asap.Github.Application.Models.Users;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(
        string firstName,
        string middleName,
        string lastName,
        CancellationToken cancellationToken);

    Task<UserDto?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);
}