using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Platform.Common.DateTime;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands.ProvisionSubjectCourse;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Handlers;

internal class ProvisionSubjectCourseHandler : IRequestHandler<Command, Response>
{
    private readonly IPersistenceContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IGithubOrganizationService _organizationService;
    private readonly IGithubRepositoryService _repositoryService;

    public ProvisionSubjectCourseHandler(
        IPersistenceContext context,
        IDateTimeProvider dateTimeProvider,
        IGithubOrganizationService organizationService,
        IGithubRepositoryService repositoryService)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _organizationService = organizationService;
        _repositoryService = repositoryService;
    }

    public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
    {
        var subjectCourseQuery = GithubSubjectCourseQuery.Build(x => x
            .WithOrganizationId(request.OrganizationId));

        GithubSubjectCourse? existingSubjectCourse = await _context.SubjectCourses
            .QueryAsync(subjectCourseQuery, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingSubjectCourse is not null)
            return new Response.OrganizationAlreadyBound();

        Response? response = await ValidateGithubEntities(request, cancellationToken);

        if (response is not null)
            return response;

        DateTimeOffset now = _dateTimeProvider.Current;

        var subjectCourse = new ProvisionedSubjectCourse(
            request.CorrelationId,
            request.OrganizationId,
            request.TemplateRepositoryId,
            request.MentorTeamId,
            now);

        _context.ProvisionedSubjectCourses.Add(subjectCourse);
        await _context.CommitAsync(cancellationToken);

        return new Response.Success();
    }

    private async Task<Response?> ValidateGithubEntities(Command command, CancellationToken cancellationToken)
    {
        GithubOrganizationModel? organization = await _organizationService
            .FindByIdAsync(command.OrganizationId, cancellationToken);

        if (organization is null)
            return new Response.OrganizationNotFound();

        GithubRepositoryModel? templateRepository = await _repositoryService
            .FindByIdAsync(command.OrganizationId, command.TemplateRepositoryId, cancellationToken);

        if (templateRepository is null)
            return new Response.TemplateRepositoryNotFound();

        if (templateRepository.IsTemplate is false)
            return new Response.TemplateRepositoryNotMarkedTemplate();

        GithubTeamModel? mentorTeam = await _organizationService
            .FindTeamAsync(command.OrganizationId, command.MentorTeamId, cancellationToken);

        return mentorTeam is null ? new Response.MentorTeamNotFound() : null;
    }
}