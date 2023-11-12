namespace Itmo.Dev.Asap.Github.Common.Tools;

public static class CommonMapper
{
    public static Guid ToGuid(this string value)
        => Guid.Parse(value);
}