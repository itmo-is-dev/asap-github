using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Notifications;
using Itmo.Dev.Asap.Github.Application.Core.Services;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Dto.Users;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Common.Exceptions.Entities;
using Itmo.Dev.Asap.Github.Common.Extensions;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using Itmo.Dev.Asap.Github.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Github.Application.Handlers.SubjectCourse;

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
            .ForOrganizationName(request.OrganizationName, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        if (subjectCourse is null)
            throw EntityNotFoundException.SubjectCourse(request.OrganizationName);

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

        IReadOnlyCollection<string> mentorsTeamMembers = await _githubOrganizationService
            .GetTeamMemberUsernamesAsync(
                subjectCourse.OrganizationName,
                subjectCourse.MentorTeamName,
                cancellationToken);

        var exitingUsersQuery = GithubUserQuery.Build(x => x.WithUsernames(mentorsTeamMembers));

        List<GithubUser> existingGithubUsers = await _context.Users
            .QueryAsync(exitingUsersQuery, cancellationToken)
            .ToListAsync(cancellationToken);

        IEnumerable<string> existingGithubUsernames = existingGithubUsers.Select(x => x.Username);

        List<GithubUser> createdUsers = await mentorsTeamMembers
            .Except(existingGithubUsernames)
            .ToAsyncEnumerable()
            .SelectAwait(async username =>
            {
                bool userExists = await _githubUserService.IsUserExistsAsync(username, default);

                if (userExists is false)
                    throw EntityNotFoundException.Create<string, GithubUser>(username).TaggedWithNotFound();

                UserDto user = await _userService.CreateUserAsync(username, username, username, default);
                return new GithubUser(user.Id, username);
            })
            .ToListAsync(default);

        _context.Users.AddRange(createdUsers);
        await _context.CommitAsync(cancellationToken);

        Guid[] userIds = existingGithubUsers.Concat(createdUsers).Select(x => x.Id).ToArray();
        await _asapSubjectCourseService.UpdateMentorsAsync(subjectCourse.Id, userIds, default);

        _logger.LogInformation("Updated github mentors for subject course {SubjectCourseId}", subjectCourse.Id);
    }
}