using CommandLine;
using Itmo.Dev.Asap.Github.Commands.CommandVisitors;
using Itmo.Dev.Asap.Github.Commands.Models;

namespace Itmo.Dev.Asap.Github.Commands.SubmissionCommands;

[Verb("/create-submission")]
public class CreateSubmissionCommand : ISubmissionCommand
{
    public string Name => "/create-submission";

    public Task<SubmissionCommandResult> AcceptAsync(ISubmissionCommandVisitor visitor)
    {
        return visitor.VisitAsync(this);
    }
}