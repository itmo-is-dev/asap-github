using Itmo.Dev.Asap.Github.Application.Models.PullRequests;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.PullRequestEvents;

public static class PullRequestCommentAdded
{
    public record Command(PullRequestDto PullRequest, long CommentId) : IRequest;
}