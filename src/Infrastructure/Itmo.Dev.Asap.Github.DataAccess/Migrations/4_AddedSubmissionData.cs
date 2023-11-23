using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.Github.DataAccess.Migrations;

#pragma warning disable SA1649

[Migration(4, "Added submission data")]
public class AddedSubmissionData : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        return """
        create table submission_data
        (
            user_id uuid not null references users(user_id),
            assignment_id uuid not null references assignments(assignment_id),
            submission_id uuid not null references submissions(submission_id),
            submission_data_task_id bigint not null ,
            submission_data_file_link text not null ,
            
            primary key (user_id, assignment_id, submission_data_task_id)
        );
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        return """
        drop table submission_data;
        """;
    }
}