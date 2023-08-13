using Itmo.Dev.Asap.Github.Application.Dto.PullRequests;
using Itmo.Dev.Asap.Github.Commands.SubmissionCommands;
using MediatR;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Commands;

public static class ExecuteSubmissionCommand
{
    public record Command(PullRequestDto PullRequest, long CommentId, ISubmissionCommand SubmissionCommand) : IRequest;
}