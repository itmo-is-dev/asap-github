using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.Time;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands.ProvisionSubjectCourse;

namespace Itmo.Dev.Asap.Github.Application.Handlers.SubjectCourse;

internal class ProvisionSubjectCourseHandler : IRequestHandler<Command>
{
    private readonly IPersistenceContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ProvisionSubjectCourseHandler(IPersistenceContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(Command request, CancellationToken cancellationToken)
    {
        DateTime now = _dateTimeProvider.Current;

        var subjectCourse = new ProvisionedSubjectCourse(
            request.CorrelationId,
            request.OrganizationName,
            request.TemplateRepositoryName,
            request.MentorTeamName,
            now);

        _context.ProvisionedSubjectCourses.Add(subjectCourse);
        await _context.CommitAsync(cancellationToken);
    }
}