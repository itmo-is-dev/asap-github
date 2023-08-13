using Itmo.Dev.Asap.Github.Application.DataAccess.Models;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Domain.Submissions;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.UnitOfWork;
using Npgsql;
using System.Runtime.CompilerServices;
using System.Text;

namespace Itmo.Dev.Asap.Github.DataAccess.Repositories;

internal class GithubSubmissionRepository : IGithubSubmissionRepository
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostgresConnectionProvider _connectionProvider;

    public GithubSubmissionRepository(IUnitOfWork unitOfWork, IPostgresConnectionProvider connectionProvider)
    {
        _unitOfWork = unitOfWork;
        _connectionProvider = connectionProvider;
    }

    public async IAsyncEnumerable<GithubSubmission> QueryAsync(
        GithubSubmissionQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string baseSql = """
        select s.submission_id, 
               s.assignment_id, 
               s.user_id, 
               s.submission_created_at, 
               s.submission_organization,
               s.submission_repository, 
               s.submission_pull_request_number
        from submissions as s
        join assignments as a using (assignment_id)
        where 
            (cardinality(:submission_ids) = 0 or s.submission_id = any(:submission_ids))
            and (cardinality(:repository_names) = 0 or s.submission_repository = any(:repository_names))
            and (cardinality(:pull_request_numbers) = 0 or s.submission_pull_request_number = any(:pull_request_numbers))
            and (cardinality(:organization_names) = 0 or s.submission_organization = any(:organization_names))
            and (cardinality(:assignment_branch_names) = 0 or a.assignment_branch_name = any(:assignment_branch_names))
        """;

        string sql = baseSql;

        if (query.HasOrderParameters)
        {
            var orders = new List<string?>
            {
                query.OrderByCreatedAt switch
                {
                    OrderDirection.Ascending => "\"CreatedAt\" asc",
                    OrderDirection.Descending => "\"CreatedAt\" desc",
                    _ => null,
                },
            };

            var builder = new StringBuilder(baseSql);
            builder.AppendLine();
            builder.Append("order by ");
            builder.AppendJoin(", ", orders.Where(x => string.IsNullOrEmpty(x) is false));

            sql = builder.ToString();
        }

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

#pragma warning disable CA2100
        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("submission_ids", query.Ids)
            .AddParameter("repository_names", query.RepositoryNames)
            .AddParameter("pull_request_numbers", query.PullRequestNumbers)
            .AddParameter("organization_names", query.OrganizationNames)
            .AddParameter("assignment_branch_names", query.AssignmentBranchNames);
#pragma warning restore CA2100

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int submissionId = reader.GetOrdinal("submission_id");
        int assignmentId = reader.GetOrdinal("assignment_id");
        int userId = reader.GetOrdinal("user_id");
        int createdAt = reader.GetOrdinal("submission_created_at");
        int organization = reader.GetOrdinal("submission_organization");
        int repository = reader.GetOrdinal("submission_repository");
        int pullRequestNumber = reader.GetOrdinal("submission_pull_request_number");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new GithubSubmission(
                reader.GetGuid(submissionId),
                reader.GetGuid(assignmentId),
                reader.GetGuid(userId),
                reader.GetDateTime(createdAt),
                reader.GetString(organization),
                reader.GetString(repository),
                reader.GetInt64(pullRequestNumber));
        }
    }

    public void Add(GithubSubmission submission)
    {
        const string sql = """
        insert into submissions
        (
            submission_id, 
            assignment_id, 
            user_id, 
            submission_created_at, 
            submission_organization, 
            submission_repository,
            submission_pull_request_number
        )
        values (:id, :assignment_id, :user_id, :created_at, :organization, :repository, :pull_request_number)
        """;

        NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("id", submission.Id)
            .AddParameter("assignment_id", submission.AssignmentId)
            .AddParameter("user_id", submission.UserId)
            .AddParameter("created_at", submission.CreatedAt)
            .AddParameter("organization", submission.Organization)
            .AddParameter("repository", submission.Repository)
            .AddParameter("pull_request_number", submission.PullRequestNumber);

        _unitOfWork.Enqueue(command);
    }
}