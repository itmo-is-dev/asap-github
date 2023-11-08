using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Commands;
using Itmo.Dev.Asap.Github.Application.Models.PullRequests;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions;

public static class ExecuteSubmissionCommand
{
    public record Command(PullRequestDto PullRequest, long CommentId, ISubmissionCommand SubmissionCommand) : IRequest;
}