namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Options;

public class StaleProvisionedSubjectCourseEraserOptions
{
    public bool IsDisabled { get; init; }

    public int DelaySeconds { get; init; }

    public int ProvisionedLifetimeSeconds { get; init; }

    public int PageSize { get; init; }

    public TimeSpan Delay => TimeSpan.FromSeconds(DelaySeconds);

    public TimeSpan ProvisionedLifetime => TimeSpan.FromSeconds(ProvisionedLifetimeSeconds);
}