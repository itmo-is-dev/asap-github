using Itmo.Dev.Asap.Github.Common.Extensions;

namespace Itmo.Dev.Asap.Github.Common.Exceptions;

public class GithubCommandException : AsapGithubException
{
    private GithubCommandException(string message) : base(message) { }

    public static AsapGithubException InvalidContext(string command, string context)
        => new GithubCommandException($"Context {context} is invalid for command {command}").TaggedWithBadRequest();
}