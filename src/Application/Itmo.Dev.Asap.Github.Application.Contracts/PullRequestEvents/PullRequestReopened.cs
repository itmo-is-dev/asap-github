using Itmo.Dev.Asap.Github.Application.Models.PullRequests;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents;

internal static class PullRequestReopened
{
    public record Command(PullRequestDto PullRequest) : IRequest;
}