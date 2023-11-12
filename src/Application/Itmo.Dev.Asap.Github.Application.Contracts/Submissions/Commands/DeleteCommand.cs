using CommandLine;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.CommandVisitors;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Models;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Commands;

[Verb("/delete")]
public class DeleteCommand : ISubmissionCommand
{
    public string Name => "/delete";

    public Task<SubmissionCommandResult> AcceptAsync(ISubmissionCommandVisitor visitor)
    {
        return visitor.VisitAsync(this);
    }
}