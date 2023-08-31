using Grpc.Core;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Presentation.Grpc.Mapping;
using Itmo.Dev.Asap.Github.Search;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Controllers;

public class GithubSearchController : GithubSearchService.GithubSearchServiceBase
{
    private readonly IGithubSearchService _service;

    public GithubSearchController(IGithubSearchService service)
    {
        _service = service;
    }

    public override async Task<SearchOrganizationsResponse> SearchOrganizations(
        SearchOrganizationsRequest request,
        ServerCallContext context)
    {
        Models.GithubOrganization[] organizations = await _service
            .SearchOrganizationsAsync(request.Query, context.CancellationToken)
            .Select(x => x.MapFrom())
            .ToArrayAsync(context.CancellationToken);

        return new SearchOrganizationsResponse { Organizations = { organizations } };
    }

    public override async Task<SearchRepositoriesResponse> SearchRepositories(
        SearchRepositoriesRequest request,
        ServerCallContext context)
    {
        Models.GithubRepository[] repositories = await _service
            .SearchOrganizationRepositoriesAsync(request.OrganizationId, request.Query, context.CancellationToken)
            .Select(x => x.MapFrom())
            .ToArrayAsync(context.CancellationToken);

        return new SearchRepositoriesResponse { Repositories = { repositories } };
    }

    public override async Task<SearchTeamsResponse> SearchTeams(SearchTeamsRequest request, ServerCallContext context)
    {
        Models.GithubTeam[] teams = await _service
            .SearchOrganizationTeamsAsync(request.OrganizationId, request.Query, context.CancellationToken)
            .Select(x => x.MapFrom())
            .ToArrayAsync(context.CancellationToken);

        return new SearchTeamsResponse { Teams = { teams } };
    }
}