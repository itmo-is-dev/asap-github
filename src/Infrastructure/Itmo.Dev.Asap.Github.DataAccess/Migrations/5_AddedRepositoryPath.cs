using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.Github.DataAccess.Migrations;

#pragma warning disable SA1649

[Migration(5, "Added assignment_repository_path")]
public class AddedRepositoryPath : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider) =>
    """
    alter table assignments
        add column assignment_repository_path text not null default '';

    alter table assignments
        alter column assignment_repository_path drop default ;
    """;

    protected override string GetDownSql(IServiceProvider serviceProvider) =>
    """
    alter table assignments
        drop column assignment_repository_path;
    """;
}