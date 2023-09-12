using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.Github.DataAccess.Migrations;

#pragma warning disable SA1649

[Migration(2, "Added subject_courses subject_course_organization_id unique index")]
public class AddedOrganizationUniqueIndex : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        return """
        create unique index subject_courses_subject_course_organization_id
        on "subject_courses"("subject_course_organization_id");
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        return """
        drop index subject_courses_subject_course_organization_id;
        """;
    }
}