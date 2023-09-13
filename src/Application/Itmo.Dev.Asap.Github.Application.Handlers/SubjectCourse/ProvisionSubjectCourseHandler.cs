using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Time;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using MediatR;
using static Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Commands.ProvisionSubjectCourse;

namespace Itmo.Dev.Asap.Github.Application.Handlers.SubjectCourse;

internal class ProvisionSubjectCourseHandler : IRequestHandler<Command, Response>
{
    private readonly IPersistenceContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ProvisionSubjectCourseHandler(IPersistenceContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
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

        DateTime now = _dateTimeProvider.Current;

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
}