using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.Github.DataAccess.Migrations;

#pragma warning disable SA1649

[Migration(1, "Initial")]
public class Initial : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        return """
        create table users
        (
            user_id uuid not null primary key,
            user_name text not null
        );

        create table subject_courses
        (
            subject_course_id uuid not null primary key,
            subject_course_organization_name text not null,
            subject_course_template_repository_name text not null,
            subject_course_mentor_team_name text not null
        );

        create table assignments
        (
            assignment_id uuid not null primary key,
            subject_course_id uuid not null references subject_courses,
            assignment_branch_name text not null
        );

        create table submissions
        (
            submission_id uuid not null primary key,
            assignment_id uuid not null references assignments,
            user_id uuid not null references users,
            submission_created_at timestamp with time zone not null,
            submission_organization text not null,
            submission_repository text not null,
            submission_pull_request_number bigint not null
        );
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        return """
        drop table assignments;
        drop table subject_courses;
        drop table submissions;
        drop table users;
        """;
    }
}