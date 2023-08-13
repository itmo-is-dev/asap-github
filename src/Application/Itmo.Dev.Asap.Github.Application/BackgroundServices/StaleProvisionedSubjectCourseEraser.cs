using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Models;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Options;
using Itmo.Dev.Asap.Github.Application.Time;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Github.Application.BackgroundServices;

public class StaleProvisionedSubjectCourseEraser : BackgroundService
{
    private readonly IOptionsMonitor<StaleProvisionedSubjectCourseEraserOptions> _options;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<StaleProvisionedSubjectCourseEraser> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public StaleProvisionedSubjectCourseEraser(
        IOptionsMonitor<StaleProvisionedSubjectCourseEraserOptions> options,
        IDateTimeProvider dateTimeProvider,
        ILogger<StaleProvisionedSubjectCourseEraser> logger,
        IServiceScopeFactory scopeFactory)
    {
        _options = options;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested is false)
        {
            StaleProvisionedSubjectCourseEraserOptions options = _options.CurrentValue;

            if (options.IsDisabled)
            {
                var tcs = new TaskCompletionSource();
                using IDisposable? subscription = _options.OnChange(_ => tcs.SetResult());

                await Task.WhenAny(tcs.Task, Task.Delay(-1, stoppingToken));
                continue;
            }

            DateTime now = _dateTimeProvider.Current;
            DateTime minAliveCreatedAt = now.Subtract(options.ProvisionedLifetime);

            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            IPersistenceContext context = scope.ServiceProvider.GetRequiredService<IPersistenceContext>();

            await RemoveAsync(context, minAliveCreatedAt, options.PageSize, stoppingToken);

            await Task.Delay(options.Delay, stoppingToken);
        }
    }

    private async Task RemoveAsync(
        IPersistenceContext context,
        DateTime createdAt,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = ProvisionedSubjectCourseQuery.Build(x => x
            .WithCursor(createdAt)
            .WithOrderDirection(OrderDirection.Descending)
            .WithPageSize(pageSize));

        while (true)
        {
            ProvisionedSubjectCourse[] staleProvisionedSubjectCourses = await context.ProvisionedSubjectCourses
                .QueryAsync(query, cancellationToken)
                .ToArrayAsync(cancellationToken);

            IEnumerable<string> correlationIds = staleProvisionedSubjectCourses.Select(x => x.CorrelationId);
            context.ProvisionedSubjectCourses.RemoveRange(correlationIds);
            await context.CommitAsync(cancellationToken);

            foreach (ProvisionedSubjectCourse subjectCourse in staleProvisionedSubjectCourses)
            {
                _logger.LogWarning(
                    "Removed stale provisioned subject course CorrelationId = {CorrelationId}, OrganizationName = {OrganizationName}",
                    subjectCourse.CorrelationId,
                    subjectCourse.OrganizationName);
            }

            if (staleProvisionedSubjectCourses.Length < pageSize)
                break;
        }
    }
}