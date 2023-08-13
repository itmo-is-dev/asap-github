using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;

internal static class ProvisionSubjectCourse
{
    public record Command(
        string CorrelationId,
        string OrganizationName,
        string TemplateRepositoryName,
        string MentorTeamName) : IRequest;
}