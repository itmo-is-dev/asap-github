using Itmo.Dev.Asap.Github.Application.Abstractions.Enrichment;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Application.Enrichment;

internal class GithubSubjectCourseEnricher : IGithubSubjectCourseEnricher
{
    private readonly IGithubOrganizationService _organizationService;
    private readonly IGithubRepositoryService _repositoryService;

    public GithubSubjectCourseEnricher(
        IGithubOrganizationService organizationService,
        IGithubRepositoryService repositoryService)
    {
        _organizationService = organizationService;
        _repositoryService = repositoryService;
    }

    public async Task<EnrichedGithubSubjectCourse> EnrichAsync(
        GithubSubjectCourse subjectCourse,
        CancellationToken cancellationToken)
    {
        GithubOrganizationModel? organization = await _organizationService
            .FindByIdAsync(subjectCourse.OrganizationId, cancellationToken);

        GithubRepositoryModel? repository = await _repositoryService.FindByIdAsync(
            subjectCourse.OrganizationId,
            subjectCourse.TemplateRepositoryId,
            cancellationToken);

        GithubTeamModel? team = await _organizationService.FindTeamAsync(
            subjectCourse.OrganizationId,
            subjectCourse.MentorTeamId,
            cancellationToken);

        return new EnrichedGithubSubjectCourse(
            subjectCourse.Id,
            subjectCourse.OrganizationId,
            organization?.Name ?? string.Empty,
            subjectCourse.TemplateRepositoryId,
            repository?.Name ?? string.Empty,
            subjectCourse.MentorTeamId,
            team?.Name ?? string.Empty);
    }
}