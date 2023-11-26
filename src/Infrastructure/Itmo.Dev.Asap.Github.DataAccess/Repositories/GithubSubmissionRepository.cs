using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Application.Models.Submissions;
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
               s.submission_organization_id,
               s.submission_repository_id, 
               s.submission_pull_request_id,
               s.submission_commit_hash
        from submissions as s
        join assignments as a using (assignment_id)
        where 
            (cardinality(:submission_ids) = 0 or s.submission_id = any(:submission_ids))
            and (cardinality(:repository_ids) = 0 or s.submission_repository_id = any(:repository_ids))
            and (cardinality(:pull_request_ids) = 0 or s.submission_pull_request_id = any(:pull_request_ids))
            and (cardinality(:organization_ids) = 0 or s.submission_organization_id = any(:organization_ids))
            and (cardinality(:assignment_branch_names) = 0 or a.assignment_branch_name = any(:assignment_branch_names))
        """;

        string sql = baseSql;

        if (query.HasOrderParameters)
        {
            var orders = new List<string?>
            {
                query.OrderByCreatedAt switch
                {
                    OrderDirection.Ascending => "\"submission_created_at\" asc",
                    OrderDirection.Descending => "\"submission_created_at\" desc",
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
            .AddParameter("repository_ids", query.RepositoryIds)
            .AddParameter("pull_request_ids", query.PullRequestIds)
            .AddParameter("organization_ids", query.OrganizationIds)
            .AddParameter("assignment_branch_names", query.AssignmentBranchNames);
#pragma warning restore CA2100

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        await foreach (GithubSubmission submission in ReadSubmissionsAsync(reader, cancellationToken))
        {
            yield return submission;
        }
    }

    public async IAsyncEnumerable<GithubSubmissionData> QueryDataAsync(
        GithubSubmissionDataQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select submission_id, 
               user_id, 
               assignment_id, 
               submission_data_task_id, 
               submission_data_file_link
        from submission_data
        where 
            submission_data_task_id = :task_id
            and (:should_not_filter_user_id or user_id >= :user_id)
            and (:should_not_filter_assignment_id or assignment_id > :assignment_id)
        order by user_id, assignment_id
        limit :limit
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("task_id", query.TaskId)
            .AddParameter("should_not_filter_user_id", query.PageToken is null)
            .AddParameter("should_not_filter_assignment_id", query.PageToken is null)
            .AddParameter("user_id", query.PageToken?.UserId ?? Guid.Empty)
            .AddParameter("assignment_id", query.PageToken?.AssignmentId ?? Guid.Empty)
            .AddParameter("limit", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int submissionId = reader.GetOrdinal("submission_id");
        int userId = reader.GetOrdinal("user_id");
        int assignmentId = reader.GetOrdinal("assignment_id");
        int taskId = reader.GetOrdinal("submission_data_task_id");
        int fileLink = reader.GetOrdinal("submission_data_file_link");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new GithubSubmissionData(
                SubmissionId: reader.GetGuid(submissionId),
                UserId: reader.GetGuid(userId),
                AssignmentId: reader.GetGuid(assignmentId),
                TaskId: reader.GetInt64(taskId),
                FileLink: reader.GetString(fileLink));
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
            submission_organization_id, 
            submission_repository_id,
            submission_pull_request_id,
            submission_commit_hash
        )
        values (:id,
                :assignment_id,
                :user_id,
                :created_at,
                :organization_id,
                :repository_id,
                :pull_request_id,
                :commit_hash)
        """;

        NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("id", submission.Id)
            .AddParameter("assignment_id", submission.AssignmentId)
            .AddParameter("user_id", submission.UserId)
            .AddParameter("created_at", submission.CreatedAt)
            .AddParameter("organization_id", submission.OrganizationId)
            .AddParameter("repository_id", submission.RepositoryId)
            .AddParameter("pull_request_id", submission.PullRequestId)
            .AddParameter("commit_hash", submission.CommitHash);

        _unitOfWork.Enqueue(command);
    }

    public void AddData(GithubSubmissionData submissionData)
    {
        const string sql = """
        insert into submission_data(submission_id, user_id, assignment_id, submission_data_task_id, submission_data_file_link) 
        values (:submission_id, :user_id, :assignment_id, :task_id, :link)
        on conflict on constraint submission_data_pkey
        do update set submission_id = excluded.submission_id,
                      submission_data_file_link = excluded.submission_data_file_link;
        """;

        using NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("submission_id", submissionData.SubmissionId)
            .AddParameter("user_id", submissionData.UserId)
            .AddParameter("assignment_id", submissionData.AssignmentId)
            .AddParameter("task_id", submissionData.TaskId)
            .AddParameter("link", submissionData.FileLink);

        _unitOfWork.Enqueue(command);
    }

    public void UpdateCommitHash(Guid submissionId, string commitHash)
    {
        const string sql = """
        update submissions 
        set submission_commit_hash = :commit_hash
        where submission_id = :id;
        """;

        using NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("id", submissionId)
            .AddParameter("commit_hash", commitHash);

        _unitOfWork.Enqueue(command);
    }

    private static async IAsyncEnumerable<GithubSubmission> ReadSubmissionsAsync(
        NpgsqlDataReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int submissionId = reader.GetOrdinal("submission_id");
        int assignmentId = reader.GetOrdinal("assignment_id");
        int userId = reader.GetOrdinal("user_id");
        int createdAt = reader.GetOrdinal("submission_created_at");
        int organization = reader.GetOrdinal("submission_organization_id");
        int repository = reader.GetOrdinal("submission_repository_id");
        int pullRequest = reader.GetOrdinal("submission_pull_request_id");
        int commitHash = reader.GetOrdinal("submission_commit_hash");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new GithubSubmission(
                Id: reader.GetGuid(submissionId),
                AssignmentId: reader.GetGuid(assignmentId),
                UserId: reader.GetGuid(userId),
                CreatedAt: reader.GetDateTime(createdAt),
                OrganizationId: reader.GetInt64(organization),
                RepositoryId: reader.GetInt64(repository),
                PullRequestId: reader.GetInt64(pullRequest),
                CommitHash: reader.GetNullableString(commitHash));
        }
    }
}