namespace Itmo.Dev.Asap.Github.Application.Time;

public class UtcNowDateTimeProvider : IDateTimeProvider
{
    public DateTime Current => DateTime.UtcNow;
}