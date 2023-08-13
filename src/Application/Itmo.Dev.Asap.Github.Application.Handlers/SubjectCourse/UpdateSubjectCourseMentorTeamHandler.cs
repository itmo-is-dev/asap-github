using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Notifications;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.Specifications;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands.UpdateSubjectCourseMentorTeam;

namespace Itmo.Dev.Asap.Github.Application.Handlers.SubjectCourse;

internal class UpdateSubjectCourseMentorTeamHandler : IRequestHandler<Command>
{
    private readonly IPersistenceContext _context;
    private readonly IPublisher _publisher;

    public UpdateSubjectCourseMentorTeamHandler(IPersistenceContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(Command request, CancellationToken cancellationToken)
    {
        GithubSubjectCourse subjectCourse = await _context.SubjectCourses
            .GetByIdAsync(request.SubjectCourseId, cancellationToken);

        subjectCourse.MentorTeamName = request.MentorsTeamName;

        _context.SubjectCourses.Update(subjectCourse);
        await _context.CommitAsync(cancellationToken);

        var notification = new SubjectCourseMentorTeamUpdated.Notification(subjectCourse.Id);
        await _publisher.Publish(notification, cancellationToken);
    }
}