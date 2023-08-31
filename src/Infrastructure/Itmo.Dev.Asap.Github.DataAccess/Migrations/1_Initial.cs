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
            user_github_id bigint not null
        );

        create unique index users_user_github_id_idx
        on "users"("user_github_id");

        create table subject_courses
        (
            subject_course_id uuid not null primary key,
            subject_course_organization_id bigint not null,
            subject_course_template_repository_id bigint not null,
            subject_course_mentor_team_id bigint not null
        );

        create table subject_course_students
        (
            subject_course_id uuid not null primary key references subject_courses,
            user_id uuid not null primary key references users,
            subject_course_student_repository_id bigint not null 
        );

        create unique index subject_course_students_subject_course_student_repository_id
        on subject_course_students(subject_course_student_repository_id);

        create table provisioned_subject_courses
        (
            correlation_id text not null ,
            created_at timestamp with time zone not null,
            subject_course_organization_id bigint not null,
            subject_course_template_repository_id bigint not null,
            subject_course_mentor_team_id bigint not null
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
            submission_organization_id bigint not null,
            submission_repository_id bigint not null,
            submission_pull_request_id bigint not null
        );
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        return """
        drop table assignments;
        drop table subject_courses;
        drop table provisioned_subject_course;
        drop table submissions;
        drop table users;
        """;
    }
}