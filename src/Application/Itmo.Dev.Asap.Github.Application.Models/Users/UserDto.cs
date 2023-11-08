namespace Itmo.Dev.Asap.Github.Application.Models.Users;

public record UserDto(Guid Id, string FirstName, string MiddleName, string LastName, int? UniversityId);