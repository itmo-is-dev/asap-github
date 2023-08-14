namespace Itmo.Dev.Asap.Github.Application.Dto.Users;

public record UserDto(Guid Id, string FirstName, string MiddleName, string LastName, int? UniversityId);