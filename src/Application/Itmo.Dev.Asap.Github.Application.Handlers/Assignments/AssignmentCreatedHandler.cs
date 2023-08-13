using Itmo.Dev.Asap.Github.Application.Contracts.Assignments.Notifications;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Domain.Assignments;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Github.Application.Handlers.Assignments;

internal class AssignmentCreatedHandler : INotificationHandler<AssignmentCreated.Notification>
{
    private readonly IPersistenceContext _context;
    private readonly ILogger<AssignmentCreatedHandler> _logger;

    public AssignmentCreatedHandler(IPersistenceContext context, ILogger<AssignmentCreatedHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(AssignmentCreated.Notification notification, CancellationToken cancellationToken)
    {
        Guid assignmentId = notification.Assignment.Id;
        string assignmentShortName = notification.Assignment.ShortName;

        var query = GithubAssignmentQuery.Build(x => x.WithId(assignmentId));

        GithubAssignment? assignment = await _context.Assignments
            .QueryAsync(query, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (assignment is not null)
        {
            assignment.SubjectCourseId = notification.Assignment.SubjectCourseId;
            assignment.BranchName = assignmentShortName;

            _logger.Log(
                LogLevel.Warning,
                "Updating github assignment that already exists, id: {AssignmentId}, name: {AssignmentShortName}",
                assignmentId,
                assignmentShortName);

            _context.Assignments.Update(assignment);
        }
        else
        {
            assignment =
                new GithubAssignment(assignmentId, notification.Assignment.SubjectCourseId, assignmentShortName);
            _context.Assignments.Add(assignment);
        }

        await _context.CommitAsync(cancellationToken);
    }
}