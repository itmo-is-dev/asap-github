using Itmo.Dev.Asap.Application.Abstractions.Mapping;
using Itmo.Dev.Asap.Github.Application.Dto.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;

namespace Itmo.Dev.Asap.Github.Application.Mapping;

internal class SubjectCourseMapper : IGithubSubjectCourseMapper
{
    private readonly IGithubOrganizationService _organizationService;
    private readonly IGithubRepositoryService _repositoryService;

    public SubjectCourseMapper(
        IGithubOrganizationService organizationService,
        IGithubRepositoryService repositoryService)
    {
        _organizationService = organizationService;
        _repositoryService = repositoryService;
    }

    public async Task<GithubSubjectCourseDto> MapAsync(
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

        return new GithubSubjectCourseDto(
            subjectCourse.Id,
            subjectCourse.OrganizationId,
            organization?.Name ?? string.Empty,
            subjectCourse.TemplateRepositoryId,
            repository?.Name ?? string.Empty,
            subjectCourse.MentorTeamId,
            team?.Name ?? string.Empty);
    }
}