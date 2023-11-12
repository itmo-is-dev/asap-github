namespace Itmo.Dev.Asap.Github.Caching.Models;

public class RedisConfiguration
{
    public string ServiceName { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string[] Endpoints { get; set; } = Array.Empty<string>();
}