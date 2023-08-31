using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Application.Dto.PullRequests;
using Itmo.Dev.Asap.Github.Common.Exceptions.Entities;
using Itmo.Dev.Asap.Github.Common.Extensions;
using Itmo.Dev.Asap.Github.Domain.Assignments;

namespace Itmo.Dev.Asap.Github.Application.Specifications;

public static class AssignmentSpecifications
{
    public static async Task<GithubAssignment> GetAssignmentForPullRequestAsync(
        this IGithubAssignmentRepository repository,
        PullRequestDto pullRequest,
        CancellationToken cancellationToken = default)
    {
        var query = GithubAssignmentQuery.Build(x => x
            .WithSubjectCourseOrganizationId(pullRequest.OrganizationId)
            .WithBranchName(pullRequest.BranchName));

        GithubAssignment? assignment = await repository
            .QueryAsync(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        return assignment ?? throw EntityNotFoundException.Assignment().TaggedWithNotFound();
    }

    public static async Task<GithubAssignment?> FindAssignmentForPullRequestAsync(
        this IGithubAssignmentRepository repository,
        PullRequestDto pullRequest,
        CancellationToken cancellationToken = default)
    {
        var query = GithubAssignmentQuery.Build(x => x
            .WithSubjectCourseOrganizationId(pullRequest.OrganizationId)
            .WithBranchName(pullRequest.BranchName));

        GithubAssignment? assignment = await repository
            .QueryAsync(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        return assignment;
    }
}