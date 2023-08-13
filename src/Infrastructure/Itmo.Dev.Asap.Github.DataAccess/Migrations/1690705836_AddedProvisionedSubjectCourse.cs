using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.Github.DataAccess.Migrations;

#pragma warning disable SA1649

[Migration(1690705836, "Added provisioned subject course")]
public class AddedProvisionedSubjectCourse : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        return """
        create table provisioned_subject_courses
        (
            correlation_id text not null ,
            created_at timestamp with time zone not null,
            subject_course_organization_name text not null,
            subject_course_template_repository_name text not null,
            subject_course_mentor_team_name text not null
        );
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        return """
        drop table provisioned_subject_course;
        """;
    }
}