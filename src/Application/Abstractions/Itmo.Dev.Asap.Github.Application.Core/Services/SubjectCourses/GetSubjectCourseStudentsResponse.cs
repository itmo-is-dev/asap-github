using Itmo.Dev.Asap.Github.Application.Core.Models;

namespace Itmo.Dev.Asap.Github.Application.Core.Services.SubjectCourses;

public record GetSubjectCourseStudentsResponse(IReadOnlyCollection<StudentDto> Students, string? PageToken);