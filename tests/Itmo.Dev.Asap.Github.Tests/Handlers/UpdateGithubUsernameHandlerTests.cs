using FluentAssertions;
using Itmo.Dev.Asap.Github.Application.Contracts.Users.Commands;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Handlers.Users;
using Itmo.Dev.Asap.Github.Application.Octokit.Models;
using Itmo.Dev.Asap.Github.Application.Octokit.Services;
using Itmo.Dev.Asap.Github.Domain.Users;
using Itmo.Dev.Platform.Testing;
using Moq;
using Xunit;

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
        var request = new UpdateGithubUsername.Command(user.Id, githubUser.Username);

        // Act
        UpdateGithubUsername.Response response = await handler.Handle(request, default);

        // Assert
        response.Should().BeOfType<UpdateGithubUsername.Response.Success>();

        persistenceContext.Verify(
            x => x.Users.Update(user),
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
        var request = new UpdateGithubUsername.Command(userId, githubUser.Username);

        // Act
        UpdateGithubUsername.Response response = await handler.Handle(request, default);

        // Assert
        response.Should().BeOfType<UpdateGithubUsername.Response.Success>();

        persistenceContext.Verify(
            x => x.Users.Add(It.Is<GithubUser>(u => u.Id.Equals(userId))),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnGithubUserNotFound_WhenGithubUserDoesNotExist()
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
        var request = new UpdateGithubUsername.Command(userId, githubUsername);

        // Act
        UpdateGithubUsername.Response response = await handler.Handle(request, default);

        // Assert
        response.Should().BeOfType<UpdateGithubUsername.Response.GithubUserNotFound>();
    }
}