using CommandLine;
using Itmo.Dev.Asap.Github.Commands.CommandVisitors;
using Itmo.Dev.Asap.Github.Commands.Models;

namespace Itmo.Dev.Asap.Github.Commands.SubmissionCommands;

[Verb("/mark-reviewed")]
public class MarkReviewedCommand : ISubmissionCommand
{
    public string Name => "/mark-reviewed";

    public Task<SubmissionCommandResult> AcceptAsync(ISubmissionCommandVisitor visitor)
    {
        return visitor.VisitAsync(this);
    }
}