using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Notifications;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Common.Exceptions.Entities;
using Itmo.Dev.Asap.Github.Common.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Handlers;

internal class SyncGithubMentorsHandler :
    IRequestHandler<SyncGithubMentors.Command>,
    INotificationHandler<SubjectCourseMentorTeamUpdated.Notification>
{
    private readonly ILogger<SyncGithubMentorsHandler> _logger;
    private readonly IGithubOrganizationService _githubOrganizationService;
    private readonly IUserService _userService;
    private readonly IGithubUserService _githubUserService;
    private readonly ISubjectCourseService _asapSubjectCourseService;
    private readonly IPersistenceContext _context;

    public SyncGithubMentorsHandler(
        ILogger<SyncGithubMentorsHandler> logger,
        IGithubOrganizationService githubOrganizationService,
        IUserService userService,
        IGithubUserService githubUserService,
        ISubjectCourseService asapSubjectCourseService,
        IPersistenceContext context)
    {
        _logger = logger;
        _githubOrganizationService = githubOrganizationService;
        _userService = userService;
        _githubUserService = githubUserService;
        _asapSubjectCourseService = asapSubjectCourseService;
        _context = context;
    }

    public async Task Handle(SyncGithubMentors.Command request, CancellationToken cancellationToken)
    {
        GithubSubjectCourse? subjectCourse = await _context.SubjectCourses
            .ForOrganization(request.OrganizationId, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        if (subjectCourse is null)
            throw EntityNotFoundException.SubjectCourse($"with organization id = {request.OrganizationId}");

        await UpdateMentorsAsync(subjectCourse, cancellationToken);
    }

    public async Task Handle(
        SubjectCourseMentorTeamUpdated.Notification notification,
        CancellationToken cancellationToken)
    {
        GithubSubjectCourse association = await _context.SubjectCourses
            .GetByIdAsync(notification.SubjectCourseId, cancellationToken);

        await UpdateMentorsAsync(association, cancellationToken);
    }

    private async Task UpdateMentorsAsync(
        GithubSubjectCourse subjectCourse,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Started updating github mentors for subject course {SubjectCourseId}",
            subjectCourse.Id);

        IReadOnlyCollection<GithubUserModel> mentorTeamMembers = await _githubOrganizationService
            .GetTeamMembersAsync(
                subjectCourse.OrganizationId,
                subjectCourse.MentorTeamId,
                cancellationToken);

        var exitingUsersQuery = GithubUserQuery.Build(x => x
            .WithGithubUserIds(mentorTeamMembers.Select(u => u.Id)));

        List<GithubUser> existingUsers = await _context.Users
            .QueryAsync(exitingUsersQuery, cancellationToken)
            .ToListAsync(cancellationToken);

        var existingUserGithubIds = existingUsers
            .Select(x => x.GithubId)
            .ToHashSet();

        List<GithubUser> createdUsers = await mentorTeamMembers
            .Where(x => existingUserGithubIds.Contains(x.Id) is false)
            .ToAsyncEnumerable()
            .SelectAwait(async model =>
            {
                GithubUserModel? githubUser = await _githubUserService.FindByIdAsync(model.Id, default);

                if (githubUser is null)
                    throw EntityNotFoundException.Create<string, GithubUser>(model.Username).TaggedWithNotFound();

                UserDto user = await _userService.CreateUserAsync(
                    model.Username,
                    model.Username,
                    model.Username,
                    default);

                return new GithubUser(user.Id, model.Id);
            })
            .ToListAsync(default);

        _context.Users.AddRange(createdUsers);
        await _context.CommitAsync(cancellationToken);

        Guid[] userIds = existingUsers.Concat(createdUsers).Select(x => x.Id).ToArray();
        await _asapSubjectCourseService.UpdateMentorsAsync(subjectCourse.Id, userIds, default);

        _logger.LogInformation("Updated github mentors for subject course {SubjectCourseId}", subjectCourse.Id);
    }
}