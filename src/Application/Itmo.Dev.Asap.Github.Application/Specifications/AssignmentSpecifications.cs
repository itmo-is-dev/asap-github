using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Application.Models.Assignments;
using Itmo.Dev.Asap.Github.Application.Models.PullRequests;

namespace Itmo.Dev.Asap.Github.Application.Specifications;

public static class AssignmentSpecifications
{
    public static async Task<GithubAssignment?> FindAssignmentForPullRequestAsync(
        this IGithubAssignmentRepository repository,
        PullRequestDto pullRequest,
        CancellationToken cancellationToken = default)
    {
        var query = GithubAssignmentQuery.Build(x => x
            .WithSubjectCourseOrganizationId(pullRequest.OrganizationId)
            .WithBranchName(pullRequest.BranchName));

        return await repository
            .QueryAsync(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);
    }
}