using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;

internal static class UpdateSubjectCourseOrganizations
{
    public record Command : IRequest;
}