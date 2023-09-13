namespace Itmo.Dev.Asap.Github.Application.Core.Services.SubjectCourses;

public record GetSubjectCourseStudentsRequest(Guid SubjectCourseId, string? PageToken, int PageSize);