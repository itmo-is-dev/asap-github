using Newtonsoft.Json;

namespace Itmo.Dev.Asap.Github.Common.Tools;

public class GithubSerializerOptions
{
    public JsonSerializerSettings Settings { get; } = new JsonSerializerSettings();
}