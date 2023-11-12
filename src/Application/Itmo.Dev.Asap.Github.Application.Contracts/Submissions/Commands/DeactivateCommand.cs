using CommandLine;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.CommandVisitors;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Models;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Commands;

[Verb("/deactivate")]
public class DeactivateCommand : ISubmissionCommand
{
    public string Name => "/deactivate";

    public Task<SubmissionCommandResult> AcceptAsync(ISubmissionCommandVisitor visitor)
    {
        return visitor.VisitAsync(this);
    }
}