using Itmo.Dev.Asap.Github.Application.Dto.PullRequests;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Asap.Github.Application.Octokit.Extensions;

public static class LoggerExtensions
{
    public static ILogger ToPullRequestLogger(this ILogger logger, PullRequestDto descriptor)
    {
        string prefix = $"{descriptor.OrganizationName}/{descriptor.RepositoryName}/{descriptor.PullRequestId}";
        return new PrefixLoggerProxy(logger, prefix);
    }
}