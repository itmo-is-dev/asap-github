using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Domain.Assignments;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.UnitOfWork;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.Github.DataAccess.Repositories;

internal class GithubAssignmentRepository : IGithubAssignmentRepository
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostgresConnectionProvider _connectionProvider;

    public GithubAssignmentRepository(IUnitOfWork unitOfWork, IPostgresConnectionProvider connectionProvider)
    {
        _unitOfWork = unitOfWork;
        _connectionProvider = connectionProvider;
    }

    public async IAsyncEnumerable<GithubAssignment> QueryAsync(
        GithubAssignmentQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select a.assignment_id, a.subject_course_id, a.assignment_branch_name 
        from assignments as a
        join subject_courses as sc using (subject_course_id)
        where 
            (cardinality(:assignment_ids) = 0 or a.assignment_id = any(:assignment_ids))
            and (cardinality(:subject_course_ids) = 0 or a.subject_course_id = any(:subject_course_ids))
            and (cardinality(:branch_names) = 0 or a.assignment_branch_name = any(:branch_names))
            and (cardinality(:subject_course_organization_ids) = 0
                or sc.subject_course_organization_id = any(:subject_course_organization_ids))
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("assignment_ids", query.Ids)
            .AddParameter("subject_course_ids", query.SubjectCourseIds)
            .AddParameter("branch_names", query.BranchNames)
            .AddParameter("subject_course_organization_ids", query.SubjectCourseOrganizationIds);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int assignmentId = reader.GetOrdinal("assignment_id");
        int subjectCourseId = reader.GetOrdinal("subject_course_id");
        int assignmentBranchName = reader.GetOrdinal("assignment_branch_name");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new GithubAssignment(
                id: reader.GetGuid(assignmentId),
                subjectCourseId: reader.GetGuid(subjectCourseId),
                branchName: reader.GetString(assignmentBranchName));
        }
    }

    public void Add(GithubAssignment assignment)
    {
        const string sql = """
        insert into assignments
        (assignment_id, subject_course_id, assignment_branch_name)
        values (:assignment_id, :subject_course_id, :assignment_branch_name)
        """;

        NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("assignment_id", assignment.Id)
            .AddParameter("subject_course_id", assignment.SubjectCourseId)
            .AddParameter("assignment_branch_name", assignment.BranchName);

        _unitOfWork.Enqueue(command);
    }

    public void Update(GithubAssignment assignment)
    {
        const string sql = """
        update "assignments"
        set subject_course_id = :subject_course_id, 
            assignment_branch_name = :assignment_branch_name
        where assignment_id = :id
        """;

        NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("assignment_id", assignment.Id)
            .AddParameter("subject_course_id", assignment.SubjectCourseId)
            .AddParameter("assignment_branch_name", assignment.BranchName);

        _unitOfWork.Enqueue(command);
    }
}