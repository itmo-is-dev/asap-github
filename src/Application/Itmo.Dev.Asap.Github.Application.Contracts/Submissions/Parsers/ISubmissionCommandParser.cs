using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Commands;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Parsers;

public interface ISubmissionCommandParser
{
    ISubmissionCommand Parse(string command);
}