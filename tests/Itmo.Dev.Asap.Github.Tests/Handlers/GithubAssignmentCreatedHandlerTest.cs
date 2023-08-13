using Itmo.Dev.Asap.Github.Application.Contracts.Assignments.Notifications;
using Itmo.Dev.Asap.Github.Application.DataAccess;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.Application.Dto.Assignments;
using Itmo.Dev.Asap.Github.Application.Handlers.Assignments;
using Itmo.Dev.Asap.Github.Domain.Assignments;
using Itmo.Dev.Platform.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Itmo.Dev.Asap.Github.Tests.Handlers;

public class GithubAssignmentCreatedHandlerTest : TestBase
{
    private readonly Mock<IPersistenceContext> _persistenceContext = new Mock<IPersistenceContext>();

    [Fact]
    public async void AddDuplicateAssignment_ShouldUpdate()
    {
        var assignmentDto = new AssignmentDto(
            Faker.Random.Guid(),
            Faker.Random.Guid(),
            "amogus",
            "amogus");

        var notification = new AssignmentCreated.Notification(assignmentDto);
        var githubAssignment = new GithubAssignment(Guid.NewGuid(), Guid.NewGuid(), assignmentDto.ShortName);

        _persistenceContext
            .Setup(context => context.Assignments.QueryAsync(It.IsAny<GithubAssignmentQuery>(), CancellationToken.None))
            .Returns(() => new List<GithubAssignment> { githubAssignment }.ToAsyncEnumerable());

        _persistenceContext
            .Setup(context => context.Assignments.Update(It.IsAny<GithubAssignment>()))
            .Verifiable();

        var handler = new AssignmentCreatedHandler(
            _persistenceContext.Object,
            NullLogger<AssignmentCreatedHandler>.Instance);

        await handler.Handle(notification, CancellationToken.None);

        Mock.VerifyAll();
    }
}