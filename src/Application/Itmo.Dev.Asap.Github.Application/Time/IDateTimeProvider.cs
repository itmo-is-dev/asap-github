namespace Itmo.Dev.Asap.Github.Application.Time;

public interface IDateTimeProvider
{
    DateTime Current { get; }
}