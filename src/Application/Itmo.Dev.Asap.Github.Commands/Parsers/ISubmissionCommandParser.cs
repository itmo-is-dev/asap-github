using Itmo.Dev.Asap.Github.Commands.SubmissionCommands;

namespace Itmo.Dev.Asap.Github.Commands.Parsers;

public interface ISubmissionCommandParser
{
    ISubmissionCommand Parse(string command);
}