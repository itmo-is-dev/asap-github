namespace Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Results;

public abstract record GetRepositoryContentResult
{
    private GetRepositoryContentResult() { }

    public sealed record Success(Stream Content) : GetRepositoryContentResult;

    public sealed record NotFound : GetRepositoryContentResult;

    public sealed record UnexpectedError(string Message) : GetRepositoryContentResult;
}