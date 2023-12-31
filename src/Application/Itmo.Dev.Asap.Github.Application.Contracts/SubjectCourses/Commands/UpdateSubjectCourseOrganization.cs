using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;

internal static class UpdateSubjectCourseOrganization
{
    public record Command(Guid SubjectCourseId) : IRequest;
}