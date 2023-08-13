using Itmo.Dev.Asap.Github.Application.Dto.PullRequests;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents;

public static class PullRequestCommentAdded
{
    public record Command(PullRequestDto PullRequest, long CommentId) : IRequest;
}