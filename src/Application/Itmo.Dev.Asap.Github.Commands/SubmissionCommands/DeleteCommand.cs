using CommandLine;
using Itmo.Dev.Asap.Github.Commands.CommandVisitors;
using Itmo.Dev.Asap.Github.Commands.Models;

namespace Itmo.Dev.Asap.Github.Commands.SubmissionCommands;

[Verb("/delete")]
public class DeleteCommand : ISubmissionCommand
{
    public string Name => "/delete";

    public Task<SubmissionCommandResult> AcceptAsync(ISubmissionCommandVisitor visitor)
    {
        return visitor.VisitAsync(this);
    }
}