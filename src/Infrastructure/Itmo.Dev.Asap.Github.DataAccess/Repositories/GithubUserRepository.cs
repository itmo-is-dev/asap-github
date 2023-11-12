using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.UnitOfWork;
using Npgsql;
using System.Runtime.CompilerServices;
using GithubUserQuery = Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries.GithubUserQuery;

namespace Itmo.Dev.Asap.Github.DataAccess.Repositories;

internal class GithubUserRepository : IGithubUserRepository
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostgresConnectionProvider _connectionProvider;

    public GithubUserRepository(IUnitOfWork unitOfWork, IPostgresConnectionProvider connectionProvider)
    {
        _unitOfWork = unitOfWork;
        _connectionProvider = connectionProvider;
    }

    public async IAsyncEnumerable<GithubUser> QueryAsync(
        GithubUserQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string baseSql = """
        select user_id, user_github_id
        from users as u
        where
            (cardinality(:user_ids) = 0 or u.user_id = any(:user_ids))
            and (cardinality(:github_ids) = 0 or u.user_github_id = any(:github_ids))
        """;

        string sql = baseSql;

        if (query.Limit is not null)
        {
            sql = $"{baseSql}\nlimit :limit";
        }

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

#pragma warning disable CA2100
        NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("user_ids", query.Ids)
            .AddParameter("github_ids", query.GithubUserIds);

        if (query.Limit is not null)
        {
            command.AddParameter("limit", query.Limit.Value);
        }
#pragma warning restore CA2100

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int userId = reader.GetOrdinal("user_id");
        int githubId = reader.GetOrdinal("user_github_id");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new GithubUser(
                reader.GetGuid(userId),
                reader.GetInt64(githubId));
        }
    }

    public void AddRange(IReadOnlyCollection<GithubUser> users)
    {
        const string sql = """
        insert into users(user_id, user_github_id)
        select user_id, github_id
        from unnest(:user_ids, :github_ids) as source(user_id, github_id);
        """;

        NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("user_ids", users.Select(x => x.Id).ToArray())
            .AddParameter("github_ids", users.Select(x => x.GithubId).ToArray());

        _unitOfWork.Enqueue(command);
    }

    public void Add(GithubUser user)
    {
        const string sql = """
        insert into users(user_id, user_github_id)
        values (:id, :github_id)
        """;

        NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("id", user.Id)
            .AddParameter("github_id", user.GithubId);

        _unitOfWork.Enqueue(command);
    }

    public void Update(GithubUser user)
    {
        const string sql = """
        update users
        set user_github_id = :github_id
        where user_id = :id
        """;

        NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("id", user.Id)
            .AddParameter("github_id", user.GithubId);

        _unitOfWork.Enqueue(command);
    }
}