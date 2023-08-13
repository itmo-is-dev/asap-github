using Xunit;

namespace Itmo.Dev.Asap.Github.Tests.Fixtures;

[CollectionDefinition(nameof(DatabaseCollectionFixture))]
public class DatabaseCollectionFixture : ICollectionFixture<GithubDatabaseFixture> { }