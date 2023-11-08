using FluentAssertions;
using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Services;
using Itmo.Dev.Asap.Github.Application.Contracts.Users.Commands;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using Itmo.Dev.Asap.Github.Application.Users;
using Itmo.Dev.Platform.Testing;
using Moq;
using Xunit;
using GithubUserQuery = Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries.GithubUserQuery;

namespace Itmo.Dev.Asap.Github.Tests.Handlers;

public class UpdateGithubUsernameHandlerTests : TestBase
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessAndUpdatesUser_WhenUserExistsAndGithubUserExists()
    {
        // Arrange
        var user = new GithubUser(Faker.Random.Guid(), Faker.Random.Long());
        var githubUser = new GithubUserModel(Faker.Random.Long(), Faker.Internet.UserName());

        var persistenceContext = new Mock<IPersistenceContext>();

        persistenceContext
            .Setup(x => x.Users.QueryAsync(It.IsAny<GithubUserQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new[] { user }.ToAsyncEnumerable);

        var userService = new Mock<IGithubUserService>();

        userService
            .Setup(x => x.FindByUsernameAsync(githubUser.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(githubUser);

        var handler = new UpdateGithubUsernameHandler(userService.Object, persistenceContext.Object);

        var request = new UpdateGithubUsernames.Command(new[]
        {
            new UpdateGithubUsernames.Command.Model(user.Id, githubUser.Username),
        });

        // Act
        UpdateGithubUsernames.Response response = await handler.Handle(request, default);

        // Assert
        response.Should().BeOfType<UpdateGithubUsernames.Response.Success>();

        persistenceContext.Verify(
            x => x.Users.Update(It.Is<GithubUser>(u => u.Id == user.Id && u.GithubId != user.GithubId)),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessAndCreateUser_WhenUserDoesNotExistAndGithubUserExist()
    {
        // Arrange
        Guid userId = Faker.Random.Guid();
        var githubUser = new GithubUserModel(Faker.Random.Long(), Faker.Internet.UserName());

        var persistenceContext = new Mock<IPersistenceContext>();

        persistenceContext
            .Setup(x => x.Users.QueryAsync(It.IsAny<GithubUserQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<GithubUser>());

        var userService = new Mock<IGithubUserService>();

        userService
            .Setup(x => x.FindByUsernameAsync(githubUser.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(githubUser);

        var handler = new UpdateGithubUsernameHandler(userService.Object, persistenceContext.Object);

        var request = new UpdateGithubUsernames.Command(new[]
        {
            new UpdateGithubUsernames.Command.Model(userId, githubUser.Username),
        });

        // Act
        UpdateGithubUsernames.Response response = await handler.Handle(request, default);

        // Assert
        response.Should().BeOfType<UpdateGithubUsernames.Response.Success>();

        persistenceContext.Verify(
            x => x.Users.Add(It.Is<GithubUser>(u => u.Id.Equals(userId))),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnGithubUsersNotFound_WhenGithubUserDoesNotExist()
    {
        // Arrange
        Guid userId = Faker.Random.Guid();
        string githubUsername = Faker.Internet.UserName();

        var persistenceContext = new Mock<IPersistenceContext>();

        var userService = new Mock<IGithubUserService>();

        userService
            .Setup(x => x.FindByUsernameAsync(githubUsername, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GithubUserModel?)null);

        var handler = new UpdateGithubUsernameHandler(userService.Object, persistenceContext.Object);

        var request = new UpdateGithubUsernames.Command(new[]
        {
            new UpdateGithubUsernames.Command.Model(userId, githubUsername),
        });

        // Act
        UpdateGithubUsernames.Response response = await handler.Handle(request, default);

        // Assert
        response.Should().BeOfType<UpdateGithubUsernames.Response.GithubUsersNotFound>();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnDuplicateUsernames_WhenModelsWithDuplicateUsernamesProvided()
    {
        // Arrange
        var persistenceContext = new Mock<IPersistenceContext>();
        var userService = new Mock<IGithubUserService>();

        var handler = new UpdateGithubUsernameHandler(userService.Object, persistenceContext.Object);

        string username = Faker.Internet.UserName();

        UpdateGithubUsernames.Command.Model[] models =
        {
            new UpdateGithubUsernames.Command.Model(Faker.Random.Guid(), username),
            new UpdateGithubUsernames.Command.Model(Faker.Random.Guid(), username),
        };

        var request = new UpdateGithubUsernames.Command(models);

        // Act
        UpdateGithubUsernames.Response response = await handler.Handle(request, default);

        // Assert
        response.Should().BeOfType<UpdateGithubUsernames.Response.DuplicateUsernames>();
    }
}