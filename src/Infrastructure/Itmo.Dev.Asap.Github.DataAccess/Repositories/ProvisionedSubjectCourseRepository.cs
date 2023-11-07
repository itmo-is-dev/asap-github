using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.UnitOfWork;
using Npgsql;
using System.Runtime.CompilerServices;
using System.Text;
using ProvisionedSubjectCourseQuery = Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries.ProvisionedSubjectCourseQuery;

namespace Itmo.Dev.Asap.Github.DataAccess.Repositories;

public class ProvisionedSubjectCourseRepository : IProvisionedSubjectCourseRepository
{
    private readonly IPostgresConnectionProvider _connectionProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ProvisionedSubjectCourseRepository(IPostgresConnectionProvider connectionProvider, IUnitOfWork unitOfWork)
    {
        _connectionProvider = connectionProvider;
        _unitOfWork = unitOfWork;
    }

    public async IAsyncEnumerable<ProvisionedSubjectCourse> QueryAsync(
        ProvisionedSubjectCourseQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string baseSql = """
        select correlation_id,
               created_at,
               subject_course_organization_id,
               subject_course_template_repository_id,
               subject_course_mentor_team_id
        from provisioned_subject_courses psc
        where 
            cardinality(:correlation_ids) = 0 or correlation_id = any (:correlation_ids)
        """;

        var sql = new StringBuilder(baseSql);

        DateTimeOffset? cursor = (query.Cursor, query.OrderDirection) switch
        {
            (null, null) => null,
            (not null, not null) => query.Cursor.Value,

            (null, not null) => query.OrderDirection is OrderDirection.Ascending
                ? DateTime.MinValue
                : DateTime.MaxValue,

            _ => null,
        };

        if (cursor is not null)
        {
            sql.AppendLine();
            sql.Append("and psc.created_at ");
            sql.Append(query.OrderDirection is OrderDirection.Ascending ? ">" : "<");
            sql.Append(" :cursor");
            sql.AppendLine();
            sql.Append("order by psc.created_at");

            if (query.OrderDirection is OrderDirection.Descending)
            {
                sql.Append(" desc");
            }

            sql.AppendLine();
        }

        if (query.PageSize is not null)
        {
            sql.AppendLine("limit :limit");
        }

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

#pragma warning disable CA2100

        await using NpgsqlCommand command = new NpgsqlCommand(sql.ToString(), connection)
            .AddParameter("correlation_ids", query.CorrelationIds);

        if (cursor is not null)
        {
            command.AddParameter("cursor", cursor.Value);
        }

        if (query.PageSize is not null)
        {
            command.AddParameter("limit", query.PageSize.Value);
        }

#pragma warning restore CA2100

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int correlationId = reader.GetOrdinal("correlation_id");
        int createdAt = reader.GetOrdinal("created_at");
        int organization = reader.GetOrdinal("subject_course_organization_id");
        int templateRepository = reader.GetOrdinal("subject_course_template_repository_id");
        int mentorTeam = reader.GetOrdinal("subject_course_mentor_team_id");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ProvisionedSubjectCourse(
                CorrelationId: reader.GetString(correlationId),
                OrganizationId: reader.GetInt32(organization),
                TemplateRepositoryId: reader.GetInt32(templateRepository),
                MentorTeamId: reader.GetInt32(mentorTeam),
                CreatedAt: reader.GetDateTime(createdAt));
        }
    }

    public void Add(ProvisionedSubjectCourse subjectCourse)
    {
        const string sql = """
        insert into provisioned_subject_courses
        (
            correlation_id,
            created_at,
            subject_course_organization_id,
            subject_course_template_repository_id,
            subject_course_mentor_team_id
        )
        values (:correlation_id, :created_at, :organization, :template_repository, :mentor_team);
        """;

        NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("correlation_id", subjectCourse.CorrelationId)
            .AddParameter("created_at", subjectCourse.CreatedAt)
            .AddParameter("organization", subjectCourse.OrganizationId)
            .AddParameter("template_repository", subjectCourse.TemplateRepositoryId)
            .AddParameter("mentor_team", subjectCourse.MentorTeamId);

        _unitOfWork.Enqueue(command);
    }

    public void RemoveRange(IEnumerable<string> correlationIds)
    {
        const string sql = """
        delete from provisioned_subject_courses
        where correlation_id = any (:correlation_ids);
        """;

        NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("correlation_ids", correlationIds.ToArray());

        _unitOfWork.Enqueue(command);
    }
}