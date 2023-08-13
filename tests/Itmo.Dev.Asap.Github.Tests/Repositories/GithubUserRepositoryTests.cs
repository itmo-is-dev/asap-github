using Dapper;
using FluentAssertions;
using Itmo.Dev.Asap.Github.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Domain.Users;
using Itmo.Dev.Asap.Github.Tests.Extensions;
using Itmo.Dev.Asap.Github.Tests.Fixtures;
using Itmo.Dev.Asap.Github.Tests.Models;
using Itmo.Dev.Platform.Postgres.UnitOfWork;
using Itmo.Dev.Platform.Testing;
using Itmo.Dev.Platform.Testing.Extensions;
using Itmo.Dev.Platform.Testing.Mocks;
using Microsoft.Extensions.Logging;
using System.Data;
using Xunit;

namespace Itmo.Dev.Asap.Github.Tests.Repositories;

[Collection(nameof(DatabaseCollectionFixture))]
public class GithubUserRepositoryTests : TestBase, IAsyncLifetime
{
    private readonly GithubDatabaseFixture _fixture;

    public GithubUserRepositoryTests(GithubDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddRange_ShouldAddManyRecords()
    {
        // Arrange
        await using PostgresConnectionProviderMock connectionProvider = _fixture.CreateConnectionProvider();
        ILogger<ReusableUnitOfWork> logger = _fixture.CreateLogger<ReusableUnitOfWork>();

        using var unit = new ReusableUnitOfWork(connectionProvider, logger);
        var repository = new GithubUserRepository(unit, connectionProvider);

        GithubUser[] users = Enumerable.Range(0, 2).Select(_ => Faker.GithubUser()).ToArray();

        // Act
        repository.AddRange(users);
        await unit.CommitAsync(IsolationLevel.ReadCommitted, default);

        // Assert
        const string sql =
            """
            select user_id as Id, user_name as Username
            from users
            """;

        IEnumerable<GithubUserModel> userModels = _fixture.Connection.Query<GithubUserModel>(sql);

        userModels.Should().BeEquivalentTo(users, x => x.ComparingByMembers<GithubUser>());
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return _fixture.ResetAsync();
    }
}