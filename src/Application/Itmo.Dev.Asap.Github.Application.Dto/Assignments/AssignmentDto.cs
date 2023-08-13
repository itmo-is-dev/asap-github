namespace Itmo.Dev.Asap.Github.Application.Dto.Assignments;

public record AssignmentDto(
    Guid SubjectCourseId,
    Guid Id,
    string Title,
    string ShortName);