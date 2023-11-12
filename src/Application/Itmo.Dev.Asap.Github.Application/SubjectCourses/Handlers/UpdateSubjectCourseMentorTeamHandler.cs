using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Notifications;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Specifications;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands.UpdateSubjectCourseMentorTeam;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Handlers;

internal class UpdateSubjectCourseMentorTeamHandler : IRequestHandler<Command, Response>
{
    private readonly IPersistenceContext _context;
    private readonly IPublisher _publisher;
    private readonly IGithubOrganizationService _organizationService;

    public UpdateSubjectCourseMentorTeamHandler(
        IPersistenceContext context,
        IPublisher publisher,
        IGithubOrganizationService organizationService)
    {
        _context = context;
        _publisher = publisher;
        _organizationService = organizationService;
    }

    public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
    {
        GithubSubjectCourse? subjectCourse = await _context.SubjectCourses
            .FindByIdAsync(request.SubjectCourseId, cancellationToken);

        if (subjectCourse is null)
            return new Response.SubjectCourseNotFound();

        GithubTeamModel? team = await _organizationService.FindTeamAsync(
            subjectCourse.OrganizationId,
            request.MentorTeamId,
            cancellationToken);

        if (team is null)
            return new Response.MentorTeamNotFound();

        subjectCourse = subjectCourse with
        {
            MentorTeamId = request.MentorTeamId,
        };

        _context.SubjectCourses.Update(subjectCourse);
        await _context.CommitAsync(cancellationToken);

        var notification = new SubjectCourseMentorTeamUpdated.Notification(subjectCourse.Id);
        await _publisher.Publish(notification, cancellationToken);

        return new Response.Success();
    }
}