using Itmo.Dev.Asap.Core.Users;
using Itmo.Dev.Asap.Github.Application.Core.Services;
using Itmo.Dev.Asap.Github.Application.Dto.Users;
using Itmo.Dev.Asap.Github.Common.Exceptions.Entities;
using Itmo.Dev.Asap.Github.Integrations.Core.Mapping;

namespace Itmo.Dev.Asap.Github.Integrations.Core.Services;

public class UserService : IUserService
{
    private readonly Asap.Core.Users.UserService.UserServiceClient _client;

    public UserService(Asap.Core.Users.UserService.UserServiceClient client)
    {
        _client = client;
    }

    public async Task<UserDto> CreateUserAsync(
        string firstName,
        string middleName,
        string lastName,
        CancellationToken cancellationToken)
    {
        var request = new CreateRequest
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
        };

        CreateResponse response = await _client.CreateAsync(request, cancellationToken: cancellationToken);

        return response.User.ToDto();
    }

    public async Task<UserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var request = new FindByIdRequest { UserId = userId.ToString() };
        FindByIdResponse response = await _client.FindByIdAsync(request, cancellationToken: cancellationToken);

        return response.UserCase is FindByIdResponse.UserOneofCase.UserValue
            ? response.UserValue.ToDto()
            : throw EntityNotFoundException.Create<Guid, UserDto>(userId);
    }
}