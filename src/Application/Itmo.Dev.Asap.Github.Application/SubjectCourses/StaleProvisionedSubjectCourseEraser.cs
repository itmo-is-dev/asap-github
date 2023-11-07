using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.SubjectCourses.Options;
using Itmo.Dev.Platform.Common.DateTime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses;

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

            DateTimeOffset now = _dateTimeProvider.Current;
            DateTimeOffset minAliveCreatedAt = now.Subtract(options.ProvisionedLifetime);

            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            IPersistenceContext context = scope.ServiceProvider.GetRequiredService<IPersistenceContext>();

            await RemoveAsync(context, minAliveCreatedAt, options.PageSize, stoppingToken);

            await Task.Delay(options.Delay, stoppingToken);
        }
    }

    private async Task RemoveAsync(
        IPersistenceContext context,
        DateTimeOffset createdAt,
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
                    "Removed stale provisioned subject course CorrelationId = {CorrelationId}, OrganizationId = {OrganizationId}",
                    subjectCourse.CorrelationId,
                    subjectCourse.OrganizationId);
            }

            if (staleProvisionedSubjectCourses.Length < pageSize)
                break;
        }
    }
}