using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.CommandVisitors;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Models;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Commands;

public interface ISubmissionCommand
{
    string Name { get; }

    Task<SubmissionCommandResult> AcceptAsync(ISubmissionCommandVisitor visitor);
}