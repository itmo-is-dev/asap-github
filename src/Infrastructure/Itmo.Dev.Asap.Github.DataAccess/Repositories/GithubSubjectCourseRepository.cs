using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.UnitOfWork;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.Github.DataAccess.Repositories;

internal class GithubSubjectCourseRepository : IGithubSubjectCourseRepository
{
    public const string QuerySql = """
    select subject_course_id, 
           subject_course_organization_name, 
           subject_course_template_repository_name, 
           subject_course_mentor_team_name
    from subject_courses as sc
    where 
        (cardinality(:subject_course_ids) = 0 or sc.subject_course_id = any(:subject_course_ids))
        and (cardinality(:organization_names) = 0 
            or lower(sc.subject_course_organization_name) = any(select lower(x) from unnest(:organization_names) as x))
        and (cardinality(:template_repository_names) = 0 
            or lower(sc.subject_course_template_repository_name) = any(select lower(x) from unnest(:template_repository_names) as x))
        and (cardinality(:mentor_team_names) = 0 
            or lower(sc.subject_course_mentor_team_name) = any(select lower(x) from unnest(:mentor_team_names) as x))
    """;

    public const string AddSql = """
    insert into subject_courses
    (
        subject_course_id, 
        subject_course_organization_name, 
        subject_course_template_repository_name, 
        subject_course_mentor_team_name
    )
    values (:id, :organization_name, :template_repository_name, :mentor_team_name)
    """;

    public const string UpdateSql = """
    update subject_courses
    set subject_course_organization_name = :organization_name, 
        subject_course_template_repository_name = :template_repository_name, 
        subject_course_mentor_team_name = :mentor_team_name 
    where subject_course_id = :id
    """;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostgresConnectionProvider _connectionProvider;

    public GithubSubjectCourseRepository(IUnitOfWork unitOfWork, IPostgresConnectionProvider connectionProvider)
    {
        _unitOfWork = unitOfWork;
        _connectionProvider = connectionProvider;
    }

    public async IAsyncEnumerable<GithubSubjectCourse> QueryAsync(
        GithubSubjectCourseQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string sql = QuerySql;

        if (query.Limit is not null)
        {
            sql = $"{sql}\nlimit :limit";
        }

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("subject_course_ids", query.Ids)
            .AddParameter("organization_names", query.OrganizationNames)
            .AddParameter("template_repository_names", query.TemplateRepositoryNames)
            .AddParameter("mentor_team_names", query.MentorTeamNames);

        if (query.Limit is not null)
        {
            command.AddParameter("limit", query.Limit.Value);
        }

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int subjectCourseId = reader.GetOrdinal("subject_course_id");
        int organization = reader.GetOrdinal("subject_course_organization_name");
        int templateRepository = reader.GetOrdinal("subject_course_template_repository_name");
        int mentorTeam = reader.GetOrdinal("subject_course_mentor_team_name");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new GithubSubjectCourse(
                reader.GetGuid(subjectCourseId),
                reader.GetString(organization),
                reader.GetString(templateRepository),
                reader.GetString(mentorTeam));
        }
    }

    public void Add(GithubSubjectCourse subjectCourse)
    {
        NpgsqlCommand command = new NpgsqlCommand(AddSql)
            .AddParameter("id", subjectCourse.Id)
            .AddParameter("organization_name", subjectCourse.OrganizationName)
            .AddParameter("template_repository_name", subjectCourse.TemplateRepositoryName)
            .AddParameter("mentor_team_name", subjectCourse.MentorTeamName);

        _unitOfWork.Enqueue(command);
    }

    public void Update(GithubSubjectCourse subjectCourse)
    {
        NpgsqlCommand command = new NpgsqlCommand(UpdateSql)
            .AddParameter("id", subjectCourse.Id)
            .AddParameter("organization_name", subjectCourse.OrganizationName)
            .AddParameter("template_repository_name", subjectCourse.TemplateRepositoryName)
            .AddParameter("mentor_team_name", subjectCourse.MentorTeamName);

        _unitOfWork.Enqueue(command);
    }
}