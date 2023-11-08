using Itmo.Dev.Asap.Github.Common.Exceptions;
using System.Net;

namespace Itmo.Dev.Asap.Github.Common.Extensions;

public static class AsapGithubExtensions
{
    public static AsapGithubException TaggedWithNotFound(this AsapGithubException exception)
        => exception.TaggedWith(HttpStatusCode.NotFound);

    private static AsapGithubException TaggedWith(this AsapGithubException exception, HttpStatusCode code)
        => new HttpTaggedException(exception, code);
}