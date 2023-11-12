using Itmo.Dev.Asap.Github.Application.Models.Users;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;

public record StudentDto(UserDto User, Guid? GroupId, string GroupName);