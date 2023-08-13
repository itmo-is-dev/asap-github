using CommandLine;
using Itmo.Dev.Asap.Github.Commands.CommandVisitors;
using Itmo.Dev.Asap.Github.Commands.Models;

namespace Itmo.Dev.Asap.Github.Commands.SubmissionCommands;

[Verb("/deactivate")]
public class DeactivateCommand : ISubmissionCommand
{
    public string Name => "/deactivate";

    public Task<SubmissionCommandResult> AcceptAsync(ISubmissionCommandVisitor visitor)
    {
        return visitor.VisitAsync(this);
    }
}