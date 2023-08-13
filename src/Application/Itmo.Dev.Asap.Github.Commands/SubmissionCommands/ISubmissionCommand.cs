using Itmo.Dev.Asap.Github.Commands.CommandVisitors;
using Itmo.Dev.Asap.Github.Commands.Models;

namespace Itmo.Dev.Asap.Github.Commands.SubmissionCommands;

public interface ISubmissionCommand
{
    string Name { get; }

    Task<SubmissionCommandResult> AcceptAsync(ISubmissionCommandVisitor visitor);
}