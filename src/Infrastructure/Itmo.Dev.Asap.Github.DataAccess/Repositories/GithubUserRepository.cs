using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Domain.Users;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.UnitOfWork;
using Npgsql;
using System.Runtime.CompilerServices;

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
        select user_id, user_name
        from users as u
        where
            (cardinality(:user_ids) = 0 or u.user_id = any(:user_ids))
            and (cardinality(:usernames) = 0 or lower(u.user_name) = any(select lower(x) from unnest(:usernames) as x))
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
            .AddParameter("usernames", query.Usernames);

        if (query.Limit is not null)
        {
            command.AddParameter("limit", query.Limit.Value);
        }
#pragma warning restore CA2100

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int userId = reader.GetOrdinal("user_id");
        int username = reader.GetOrdinal("user_name");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new GithubUser(
                reader.GetGuid(userId),
                reader.GetString(username));
        }
    }

    public void AddRange(IReadOnlyCollection<GithubUser> users)
    {
        const string sql = """
        insert into users(user_id, user_name)
        select user_id, user_name
        from unnest(:user_ids, :user_names) as source(user_id, user_name);
        """;

        NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("user_ids", users.Select(x => x.Id).ToArray())
            .AddParameter("user_names", users.Select(x => x.Username).ToArray());

        _unitOfWork.Enqueue(command);
    }

    public void Add(GithubUser user)
    {
        const string sql = """
        insert into users (user_id, user_name)
        values (:id, :name)
        """;

        NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("id", user.Id)
            .AddParameter("name", user.Username);

        _unitOfWork.Enqueue(command);
    }

    public void Update(GithubUser user)
    {
        const string sql = """
        update users
        set user_name = :name
        where user_id = :id
        """;

        NpgsqlCommand command = new NpgsqlCommand(sql)
            .AddParameter("id", user.Id)
            .AddParameter("name", user.Username);

        _unitOfWork.Enqueue(command);
    }
}