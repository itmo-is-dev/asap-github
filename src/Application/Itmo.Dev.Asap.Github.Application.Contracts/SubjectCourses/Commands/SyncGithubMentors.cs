using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;

internal static class SyncGithubMentors
{
    public record Command(long OrganizationId) : IRequest;
}