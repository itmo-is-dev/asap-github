using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Clients;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Results;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Octokit;
using System.Net;

namespace Itmo.Dev.Asap.Github.Octokit.Services;

internal class GithubContentService : IGithubContentService
{
    private readonly IGithubClientProvider _clientProvider;

    public GithubContentService(IGithubClientProvider clientProvider)
    {
        _clientProvider = clientProvider;
    }

    public async Task<GetRepositoryContentResult> GetRepositoryContentAsync(
        long organizationId,
        long repositoryId,
        string hash,
        CancellationToken cancellationToken)
    {
        IGitHubClient client = await _clientProvider.GetOrganizationClientAsync(organizationId, cancellationToken);

        try
        {
            byte[] response = await client.Repository.Content.GetArchive(repositoryId, ArchiveFormat.Zipball, hash);
            var ms = new MemoryStream(response);

            return new GetRepositoryContentResult.Success(ms);
        }
        catch (ApiException e) when (e.StatusCode is HttpStatusCode.NotFound)
        {
            return new GetRepositoryContentResult.NotFound();
        }
        catch (Exception e)
        {
            return new GetRepositoryContentResult.UnexpectedError(e.Message);
        }
    }
}