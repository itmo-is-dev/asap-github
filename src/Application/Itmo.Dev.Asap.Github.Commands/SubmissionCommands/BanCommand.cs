using CommandLine;
using Itmo.Dev.Asap.Github.Commands.CommandVisitors;
using Itmo.Dev.Asap.Github.Commands.Models;

namespace Itmo.Dev.Asap.Github.Commands.SubmissionCommands;

[Verb("/ban")]
public class BanCommand : ISubmissionCommand
{
    public BanCommand(int? submissionCode)
    {
        SubmissionCode = submissionCode;
    }

    [Value(0, Required = false, MetaName = "SubmissionCode")]
    public int? SubmissionCode { get; }

    public string Name => "/ban";

    public Task<SubmissionCommandResult> AcceptAsync(ISubmissionCommandVisitor visitor)
    {
        return visitor.VisitAsync(this);
    }
}