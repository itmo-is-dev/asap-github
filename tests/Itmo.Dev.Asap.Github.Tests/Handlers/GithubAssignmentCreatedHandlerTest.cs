using Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess;
using Itmo.Dev.Asap.Github.Application.Assignments;
using Itmo.Dev.Asap.Github.Application.Contracts.Assignments.Notifications;
using Itmo.Dev.Asap.Github.Application.Models.Assignments;
using Itmo.Dev.Platform.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using GithubAssignmentQuery = Itmo.Dev.Asap.Github.Application.Abstractions.DataAccess.Queries.GithubAssignmentQuery;

namespace Itmo.Dev.Asap.Github.Tests.Handlers;

public class GithubAssignmentCreatedHandlerTest : TestBase
{
    private readonly Mock<IPersistenceContext> _persistenceContext = new Mock<IPersistenceContext>();

    [Fact]
    public async void AddDuplicateAssignment_ShouldUpdate()
    {
        var assignment = new GithubAssignment(
            Id: Faker.Random.Guid(),
            SubjectCourseId: Faker.Random.Guid(),
            BranchName: "amogus",
            RepositoryPath: string.Empty);

        var notification = new AssignmentCreated.Notification(assignment);

        _persistenceContext
            .Setup(context => context.Assignments.QueryAsync(It.IsAny<GithubAssignmentQuery>(), CancellationToken.None))
            .Returns(() => new List<GithubAssignment> { assignment }.ToAsyncEnumerable());

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