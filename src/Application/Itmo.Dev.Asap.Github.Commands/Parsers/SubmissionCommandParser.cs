using CommandLine;
using FluentScanning;
using Itmo.Dev.Asap.Github.Commands.SubmissionCommands;

namespace Itmo.Dev.Asap.Github.Commands.Parsers;

public class SubmissionCommandParser : ISubmissionCommandParser
{
    private static readonly char[] ArgumentsSeparators = { ' ' };

    private readonly Type[] _commandTypes;

    public SubmissionCommandParser()
    {
        _commandTypes = new AssemblyScanner(typeof(IAssemblyMarker))
            .ScanForTypesThat()
            .AreAssignableTo<ISubmissionCommand>()
            .AreNotInterfaces()
            .AreNotAbstractClasses()
            .AsTypes()
            .ToArray();
    }

    public ISubmissionCommand Parse(string command)
    {
        ParserResult<object> result = Parser.Default.ParseArguments(GetCommandArguments(command), _commandTypes);

        if (result.Tag is ParserResultType.NotParsed)
            throw new InvalidOperationException();

        return (ISubmissionCommand)result.Value;
    }

    private static IEnumerable<string> GetCommandArguments(string command)
    {
        const StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        return command.Split(ArgumentsSeparators, splitOptions);
    }
}