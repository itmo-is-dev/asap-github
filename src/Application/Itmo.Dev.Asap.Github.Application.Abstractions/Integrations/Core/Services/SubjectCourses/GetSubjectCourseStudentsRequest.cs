namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubjectCourses;

public record GetSubjectCourseStudentsRequest(Guid SubjectCourseId, string? PageToken, int PageSize);