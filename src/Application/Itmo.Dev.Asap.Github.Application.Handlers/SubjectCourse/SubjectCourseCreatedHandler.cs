using Itmo.Dev.Asap.Github.Application.Contracts.SubjectCourses.Notifications;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Extensions;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Github.Application.Handlers.SubjectCourse;

internal class SubjectCourseCreatedHandler : INotificationHandler<SubjectCourseCreated.Notification>
{
    private readonly IPersistenceContext _context;
    private readonly ILogger<SubjectCourseCreatedHandler> _logger;

    public SubjectCourseCreatedHandler(IPersistenceContext context, ILogger<SubjectCourseCreatedHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(SubjectCourseCreated.Notification notification, CancellationToken cancellationToken)
    {
        if (notification.CorrelationId is null)
        {
            _logger.LogWarning(
                "No correlation id specified for created subject course with SubjectCourseId = {SubjectCourseId}",
                notification.SubjectCourse.Id);

            return;
        }

        var provisionedQuery = ProvisionedSubjectCourseQuery.Build(x => x
            .WithCorrelationId(notification.CorrelationId));

        ProvisionedSubjectCourse? provisionedSubjectCourse = await _context.ProvisionedSubjectCourses
            .QueryAsync(provisionedQuery, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        if (provisionedSubjectCourse is null)
        {
            _logger.LogWarning(
                "No provisioned subject course found for CorrelationId = {CorrelationId}, SubjectCourseId = {SubjectCourseId}",
                notification.CorrelationId,
                notification.SubjectCourse.Id);

            return;
        }

        var subjectCourse = new GithubSubjectCourse(
            notification.SubjectCourse.Id,
            provisionedSubjectCourse.OrganizationName,
            provisionedSubjectCourse.TemplateRepositoryName,
            provisionedSubjectCourse.MentorTeamName);

        _context.SubjectCourses.Add(subjectCourse);
        _context.ProvisionedSubjectCourses.Remove(provisionedSubjectCourse.CorrelationId);

        await _context.CommitAsync(cancellationToken);
    }
}