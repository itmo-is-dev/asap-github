namespace Itmo.Dev.Asap.Github.Application.Core.Exceptions;

public class AsapCoreException : Exception
{
    public AsapCoreException() { }

    public AsapCoreException(string? message) : base(message) { }

    public AsapCoreException(string? message, Exception? innerException) : base(message, innerException) { }
}