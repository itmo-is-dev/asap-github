using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.Github.DataAccess.Migrations;

#pragma warning disable SA1649

[Migration(3, "Add submissions submission_commit_hash column")]
public class AddedSubmissionCommitHash : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        return """
        alter table submissions
            add column submission_commit_hash text;
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        return """
        alter table submissions
            drop column submission_commit_hash;
        """;
    }
}