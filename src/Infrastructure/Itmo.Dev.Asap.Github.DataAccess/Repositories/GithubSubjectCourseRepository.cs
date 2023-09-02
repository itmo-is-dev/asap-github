using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using Itmo.Dev.Asap.Github.Domain.Users;
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
           subject_course_organization_id, 
           subject_course_template_repository_id, 
           subject_course_mentor_team_id
    from subject_courses as sc
    where 
        (cardinality(:subject_course_ids) = 0 or sc.subject_course_id = any(:subject_course_ids))
        and (cardinality(:organization_ids) = 0 or sc.subject_course_organization_id = any(:organization_ids))
        and (cardinality(:template_repository_ids) = 0 or sc.subject_course_template_repository_id = any(:template_repository_ids))
        and (cardinality(:mentor_team_ids) = 0 or sc.subject_course_mentor_team_id = any(:mentor_team_ids))
    """;

    public const string QueryStudentsSql = """
    select s.subject_course_id subject_course_id,
           u.user_id user_id,
           u.user_github_id user_github_id,
           s.subject_course_student_repository_id subject_course_student_repository_id
    from subject_course_students s
    join users u on (user_id)
    where 
        (cardinality(:subject_course_ids) = 0 or s.subject_course_id = any (:subject_course_ids))
    """;

    public const string AddSql = """
    insert into subject_courses
    (
        subject_course_id, 
        subject_course_organization_id, 
        subject_course_template_repository_id, 
        subject_course_mentor_team_id
    )
    values (:id, :organization_id, :template_repository_id, :mentor_team_id)
    """;

    public const string AddStudentSql = """
    insert into subject_course_students
    (subject_course_id, user_id, subject_course_student_repository_id)
    values (:subject_course_id, :user_id, :repository_id);
    """;

    public const string UpdateSql = """
    update subject_courses
    set subject_course_organization_id = :organization_id, 
        subject_course_template_repository_id = :template_repository_id, 
        subject_course_mentor_team_id = :mentor_team_id
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

#pragma warning disable CA2100
        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("subject_course_ids", query.Ids)
            .AddParameter("organization_ids", query.OrganizationIds)
            .AddParameter("template_repository_ids", query.TemplateRepositoryIds)
            .AddParameter("mentor_team_ids", query.MentorTeamIds);
#pragma warning restore CA2100

        if (query.Limit is not null)
        {
            command.AddParameter("limit", query.Limit.Value);
        }

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int subjectCourseId = reader.GetOrdinal("subject_course_id");
        int organization = reader.GetOrdinal("subject_course_organization_id");
        int templateRepository = reader.GetOrdinal("subject_course_template_repository_id");
        int mentorTeam = reader.GetOrdinal("subject_course_mentor_team_id");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new GithubSubjectCourse(
                id: reader.GetGuid(subjectCourseId),
                organizationId: reader.GetInt64(organization),
                templateRepositoryId: reader.GetInt64(templateRepository),
                mentorTeamId: reader.GetInt64(mentorTeam));
        }
    }

    public async IAsyncEnumerable<GithubSubjectCourseStudent> QueryStudentsAsync(
        GithubSubjectCourseStudentQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(QueryStudentsSql, connection)
            .AddParameter("subject_course_ids", query.SubjectCourseIds);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int subjectCourseId = reader.GetOrdinal("subject_course_id");
        int userId = reader.GetOrdinal("user_id");
        int userGithubId = reader.GetOrdinal("user_github_id");
        int repositoryId = reader.GetOrdinal("subject_course_student_repository_id");

        while (await reader.ReadAsync(cancellationToken))
        {
            var user = new GithubUser(
                id: reader.GetGuid(userId),
                githubId: reader.GetInt64(userGithubId));

            yield return new GithubSubjectCourseStudent(
                SubjectCourseId: reader.GetGuid(subjectCourseId),
                User: user,
                RepositoryId: reader.GetInt64(repositoryId));
        }
    }

    public void Add(GithubSubjectCourse subjectCourse)
    {
        NpgsqlCommand command = new NpgsqlCommand(AddSql)
            .AddParameter("id", subjectCourse.Id)
            .AddParameter("organization_id", subjectCourse.OrganizationId)
            .AddParameter("template_repository_id", subjectCourse.TemplateRepositoryId)
            .AddParameter("mentor_team_id", subjectCourse.MentorTeamId);

        _unitOfWork.Enqueue(command);
    }

    public void AddStudent(GithubSubjectCourseStudent student)
    {
        NpgsqlCommand command = new NpgsqlCommand(AddStudentSql)
            .AddParameter("subject_course_id", student.SubjectCourseId)
            .AddParameter("user_id", student.User.Id)
            .AddParameter("repository_id", student.RepositoryId);

        _unitOfWork.Enqueue(command);
    }

    public void Update(GithubSubjectCourse subjectCourse)
    {
        NpgsqlCommand command = new NpgsqlCommand(UpdateSql)
            .AddParameter("id", subjectCourse.Id)
            .AddParameter("organization_id", subjectCourse.OrganizationId)
            .AddParameter("template_repository_id", subjectCourse.TemplateRepositoryId)
            .AddParameter("mentor_team_id", subjectCourse.MentorTeamId);

        _unitOfWork.Enqueue(command);
    }
}