using Itmo.Dev.Asap.Github.Application.Dto.PullRequests;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents;

internal static class PullRequestApproved
{
    public record Command(PullRequestDto PullRequest) : IRequest;
}