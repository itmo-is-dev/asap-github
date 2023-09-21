using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;
using Itmo.Dev.Asap.Github.Application.Core.Services.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Handlers.Configuration;
using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using Itmo.Dev.Asap.Github.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Github.Application.Handlers.SubjectCourse;

internal class SubjectCourseOrganizationUpdateHandler :
    IRequestHandler<UpdateSubjectCourseOrganizations.Command>,
    IRequestHandler<UpdateSubjectCourseOrganization.Command>
{
    private readonly ILogger<SubjectCourseOrganizationUpdateHandler> _logger;
    private readonly ISubjectCourseService _asapSubjectCourseService;
    private readonly IGithubOrganizationService _githubOrganizationService;
    private readonly IGithubRepositoryService _githubRepositoryService;
    private readonly IPersistenceContext _context;
    private readonly IGithubUserService _githubUserService;
    private readonly SubjectCourseOrganizationUpdateConfiguration _configuration;

    public SubjectCourseOrganizationUpdateHandler(
        ILogger<SubjectCourseOrganizationUpdateHandler> logger,
        ISubjectCourseService asapSubjectCourseService,
        IGithubOrganizationService githubOrganizationService,
        IGithubRepositoryService githubRepositoryService,
        IPersistenceContext context,
        IGithubUserService githubUserService,
        IOptions<SubjectCourseOrganizationUpdateConfiguration> configuration)
    {
        _logger = logger;
        _asapSubjectCourseService = asapSubjectCourseService;
        _githubOrganizationService = githubOrganizationService;
        _githubRepositoryService = githubRepositoryService;
        _context = context;
        _githubUserService = githubUserService;
        _configuration = configuration.Value;
    }

    public async Task Handle(UpdateSubjectCourseOrganizations.Command request, CancellationToken cancellationToken)
    {
        GithubSubjectCourse[] subjectCourses = await _context.SubjectCourses
            .QueryAsync(GithubSubjectCourseQuery.Build(builder => builder), cancellationToken)
            .ToArrayAsync(cancellationToken);

        foreach (GithubSubjectCourse subjectCourse in subjectCourses)
        {
            await UpdateOrganizationSafeAsync(subjectCourse, cancellationToken);
        }
    }

    public async Task Handle(UpdateSubjectCourseOrganization.Command request, CancellationToken cancellationToken)
    {
        var query = GithubSubjectCourseQuery.Build(x => x.WithId(request.SubjectCourseId).WithLimit(2));

        GithubSubjectCourse subjectCourse = await _context.SubjectCourses
            .QueryAsync(query, cancellationToken)
            .SingleAsync(cancellationToken);

        await UpdateOrganizationSafeAsync(subjectCourse, cancellationToken);
    }

    private async Task UpdateOrganizationSafeAsync(
        GithubSubjectCourse subjectCourse,
        CancellationToken cancellationToken)
    {
        try
        {
            await UpdateOrganizationAsync(subjectCourse, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Failed to update organization for subject course = {SubjectCourseId}, organization = {OrganizationId}",
                subjectCourse.Id,
                subjectCourse.OrganizationId);
        }
    }

    private async Task UpdateOrganizationAsync(GithubSubjectCourse subjectCourse, CancellationToken cancellationToken)
    {
        var request = new GetSubjectCourseStudentsRequest(subjectCourse.Id, null, _configuration.StudentPageSize);

        do
        {
            GetSubjectCourseStudentsResponse response = await _asapSubjectCourseService
                .GetSubjectCourseStudents(request, cancellationToken);

            request = request with { PageToken = response.PageToken };

            _logger.LogInformation(
                "Received students, count = {Count}, page token = {PageToken}",
                response.Students.Count,
                response.PageToken);

            Guid[] studentIds = response.Students
                .Select(x => x.User.Id)
                .ToArray();

            if (studentIds is [])
                break;

            await UpdateOrganizationBatchAsync(subjectCourse, studentIds, cancellationToken);
        }
        while (request.PageToken is not null);
    }

    private async Task UpdateOrganizationBatchAsync(
        GithubSubjectCourse subjectCourse,
        IReadOnlyCollection<Guid> studentIds,
        CancellationToken cancellationToken)
    {
        var githubUserQuery = GithubUserQuery.Build(x => x.WithIds(studentIds));

        GithubUser[] students = await _context.Users
            .QueryAsync(githubUserQuery, cancellationToken)
            .ToArrayAsync(cancellationToken);

        var existingStudentsQuery = GithubSubjectCourseStudentQuery.Build(x => x
            .WithSubjectCourseId(subjectCourse.Id)
            .WithUserIds(studentIds));

        Dictionary<Guid, GithubSubjectCourseStudent> existingStudents = await _context.SubjectCourses
            .QueryStudentsAsync(existingStudentsQuery, cancellationToken)
            .ToDictionaryAsync(x => x.User.Id, cancellationToken);

        _logger.LogInformation(
            "Started repository generation for organization {OrganizationId}",
            subjectCourse.OrganizationId);

        await GenerateRepositories(subjectCourse, students, existingStudents, cancellationToken);

        _logger.LogInformation(
            "Finished repository generation for organization {OrganizationId}",
            subjectCourse.OrganizationId);
    }

    private async ValueTask GenerateRepositories(
        GithubSubjectCourse subjectCourse,
        IEnumerable<GithubUser> students,
        IReadOnlyDictionary<Guid, GithubSubjectCourseStudent> existingStudents,
        CancellationToken cancellationToken)
    {
        GithubRepositoryModel? templateRepository = await _githubRepositoryService.FindByIdAsync(
            subjectCourse.OrganizationId,
            subjectCourse.TemplateRepositoryId,
            cancellationToken);

        if (templateRepository is null)
        {
            _logger.LogWarning(
                "No template repository found for organization = {OrganizationId}",
                subjectCourse.OrganizationId);

            return;
        }

        GithubTeamModel? team = await _githubOrganizationService.FindTeamAsync(
            subjectCourse.OrganizationId,
            subjectCourse.MentorTeamId,
            cancellationToken);

        if (team is null)
        {
            _logger.LogWarning(
                "No mentor team found for organization = {OrganizationId}",
                subjectCourse.OrganizationId);

            return;
        }

        foreach (GithubUser student in students)
        {
            if (existingStudents.TryGetValue(student.Id, out GithubSubjectCourseStudent? existingStudent))
            {
                await TryAddUserPermissionsAsync(subjectCourse, existingStudent, cancellationToken);
                continue;
            }

            long? createdRepositoryId = await TryCreateRepositoryFromTemplateAsync(
                subjectCourse,
                student,
                cancellationToken);

            if (createdRepositoryId is null)
                continue;

            var subjectCourseStudent = new GithubSubjectCourseStudent(
                subjectCourse.Id,
                student,
                createdRepositoryId.Value);

            _context.SubjectCourses.AddStudent(subjectCourseStudent);

            await AddTeamPermissionAsync(subjectCourse, createdRepositoryId.Value, cancellationToken);

            bool userPermissionsAdded = await TryAddUserPermissionsAsync(
                subjectCourse,
                subjectCourseStudent,
                cancellationToken);

            if (userPermissionsAdded is false)
                continue;

            _logger.LogInformation("Successfully created repository for user {UserId}", student.Id);
        }

        await _context.CommitAsync(cancellationToken);
    }

    private async Task AddTeamPermissionAsync(
        GithubSubjectCourse subjectCourse,
        long repositoryId,
        CancellationToken cancellationToken)
    {
        const RepositoryPermission permission = RepositoryPermission.Maintain;

        await _githubRepositoryService.AddTeamPermissionAsync(
            subjectCourse.OrganizationId,
            repositoryId,
            subjectCourse.MentorTeamId,
            permission,
            cancellationToken);
    }

    private async Task<long?> TryCreateRepositoryFromTemplateAsync(
        GithubSubjectCourse subjectCourse,
        GithubUser user,
        CancellationToken cancellationToken)
    {
        try
        {
            GithubUserModel? model = await _githubUserService.FindByIdAsync(user.GithubId, cancellationToken);

            if (model is null)
                return null;

            return await _githubRepositoryService.CreateRepositoryFromTemplateAsync(
                subjectCourse.OrganizationId,
                model.Username,
                subjectCourse.TemplateRepositoryId,
                cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create repo for user = {UserId}", user.Id);
            return null;
        }
    }

    private async Task<bool> TryAddUserPermissionsAsync(
        GithubSubjectCourse subjectCourse,
        GithubSubjectCourseStudent student,
        CancellationToken cancellationToken)
    {
        const RepositoryPermission permission = RepositoryPermission.Push;

        try
        {
            AddPermissionResult result = await _githubRepositoryService.AddUserPermissionAsync(
                subjectCourse.OrganizationId,
                student.RepositoryId,
                student.User.GithubId,
                permission,
                cancellationToken);

            return result is not AddPermissionResult.Failed;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to add user = {UserId} to repository", student.User.Id);
            return false;
        }
    }
}