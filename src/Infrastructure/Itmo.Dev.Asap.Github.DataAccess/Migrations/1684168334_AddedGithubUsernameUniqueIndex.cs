using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.Github.DataAccess.Migrations;

#pragma warning disable SA1649

[Migration(1684168334, "Added GithubUsers.Username unique index")]
public class AddedGithubUsernameUniqueIndex : SqlMigration
{
    public const string IndexName = "user_username";

    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        return $"""
        create unique index {IndexName}
        on "users"("user_name")
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        return $"""
        drop index {IndexName}
        """;
    }
}