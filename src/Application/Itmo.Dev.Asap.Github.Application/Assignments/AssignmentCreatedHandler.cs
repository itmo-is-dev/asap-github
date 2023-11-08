using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Contracts.Assignments.Notifications;
using Itmo.Dev.Asap.Github.Application.Models.Assignments;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Github.Application.Assignments;

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
        var query = GithubAssignmentQuery.Build(x => x.WithId(notification.Assignment.Id));

        GithubAssignment? assignment = await _context.Assignments
            .QueryAsync(query, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (assignment is not null)
        {
            assignment = assignment with
            {
                SubjectCourseId = notification.Assignment.SubjectCourseId,
                BranchName = notification.Assignment.BranchName,
            };

            _logger.Log(
                LogLevel.Warning,
                "Updating github assignment that already exists, id: {AssignmentId}, name: {AssignmentShortName}",
                notification.Assignment.Id,
                notification.Assignment.BranchName);

            _context.Assignments.Update(assignment);
        }
        else
        {
            _context.Assignments.Add(notification.Assignment);
        }

        await _context.CommitAsync(cancellationToken);
    }
}