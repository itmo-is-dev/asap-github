using Itmo.Dev.Asap.Github.Application.Dto.Users;

namespace Itmo.Dev.Asap.Github.Application.Core.Models;

public record StudentDto(UserDto User, Guid? GroupId, string GroupName);