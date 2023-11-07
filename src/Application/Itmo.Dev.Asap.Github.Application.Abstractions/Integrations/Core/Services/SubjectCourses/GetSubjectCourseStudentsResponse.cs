using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;

namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubjectCourses;

public record GetSubjectCourseStudentsResponse(IReadOnlyCollection<StudentDto> Students, string? PageToken);