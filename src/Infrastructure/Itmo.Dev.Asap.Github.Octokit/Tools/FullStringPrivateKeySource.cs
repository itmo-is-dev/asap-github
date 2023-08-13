using GitHubJwt;

namespace Itmo.Dev.Asap.Github.Octokit.Tools;

public class FullStringPrivateKeySource : IPrivateKeySource
{
    private readonly string _value;

    public FullStringPrivateKeySource(string value)
    {
        _value = value;
    }

    public TextReader GetPrivateKeyReader()
    {
        return new StringReader(_value);
    }
}