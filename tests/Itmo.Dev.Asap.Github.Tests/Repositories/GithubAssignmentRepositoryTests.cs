using Dapper;
using FluentAssertions;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.DataAccess.Repositories;
using Itmo.Dev.Asap.Github.Domain.Assignments;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
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
using Xunit.Abstractions;

namespace Itmo.Dev.Asap.Github.Tests.Repositories;

[Collection(nameof(DatabaseCollectionFixture))]
public class GithubAssignmentRepositoryTests : TestBase, IAsyncLifetime
{
    private readonly GithubDatabaseFixture _fixture;

    public GithubAssignmentRepositoryTests(GithubDatabaseFixture fixture, ITestOutputHelper output) : base(output)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Add_ShouldAddDatabaseRecordCorrectly()
    {
        // Arrange
        await using PostgresConnectionProviderMock connectionProvider = _fixture.CreateConnectionProvider();
        ILogger<ReusableUnitOfWork> logger = _fixture.CreateLogger<ReusableUnitOfWork>();

        using var unit = new ReusableUnitOfWork(connectionProvider, logger);
        var subjectCourseRepository = new GithubSubjectCourseRepository(unit, connectionProvider);
        var assignmentRepository = new GithubAssignmentRepository(unit, connectionProvider);

        GithubSubjectCourse subjectCourse = Faker.GithubSubjectCourse();
        GithubAssignment assignment = Faker.GithubAssignment(subjectCourseId: subjectCourse.Id);

        // Act
        subjectCourseRepository.Add(subjectCourse);
        assignmentRepository.Add(assignment);
        await unit.CommitAsync(IsolationLevel.ReadCommitted, default);

        // Assert
        const string sql = """
        select assignment_id as Id, subject_course_id as SubjectCourseId, assignment_branch_name as BranchName
        from assignments
        """;

        GithubAssignmentModel[] assignments = _fixture.Connection.Query<GithubAssignmentModel>(sql).ToArray();

        GithubAssignmentModel assignmentModel = assignments.Should().ContainSingle().Subject;
        assignmentModel.Should().NotBeEquivalentTo(assignment);
    }

    [Fact]
    public async Task QueryAsync_ShouldReturnCorrectRecords()
    {
        // Arrange
        await using PostgresConnectionProviderMock connectionProvider = _fixture.CreateConnectionProvider();
        ILogger<ReusableUnitOfWork> logger = _fixture.CreateLogger<ReusableUnitOfWork>();

        using var unit = new ReusableUnitOfWork(connectionProvider, logger);
        var repository = new GithubAssignmentRepository(unit, connectionProvider);

        Guid subjectCourseId = Faker.Random.Guid();
        Guid subjectCourseId2 = Faker.Random.Guid();
        Guid subjectCourseId3 = Faker.Random.Guid();
        string subjectCourseName = Faker.Company.CompanyName();

        GithubSubjectCourseModel subjectCourse = Faker.GithubSubjectCourseModel(id: subjectCourseId);

        var args = new
        {
            id = subjectCourse.Id,
            organization_name = subjectCourse.OrganizationName,
            template_repository_name = subjectCourse.TemplateRepositoryName,
            mentor_team_name = subjectCourse.MentorTeamName,
        };

        var args2 = args with { id = subjectCourseId2 };
        var args3 = args with { id = subjectCourseId3, organization_name = subjectCourseName };

        await _fixture.Connection.ExecuteAsync(GithubSubjectCourseRepository.AddSql, args);
        await _fixture.Connection.ExecuteAsync(GithubSubjectCourseRepository.AddSql, args2);
        await _fixture.Connection.ExecuteAsync(GithubSubjectCourseRepository.AddSql, args3);

        GithubAssignment[] seedAssignments =
        {
            Faker.GithubAssignment(subjectCourseId: subjectCourseId),
            Faker.GithubAssignment(subjectCourseId: subjectCourseId2),
            Faker.GithubAssignment(subjectCourseId: subjectCourseId),
            Faker.GithubAssignment(subjectCourseId: subjectCourseId3),
        };

        foreach (GithubAssignment assignment in seedAssignments)
        {
            repository.Add(assignment);
        }

        await unit.CommitAsync(IsolationLevel.ReadCommitted, default);

        var query1 = GithubAssignmentQuery.Build(x => x.WithId(seedAssignments[0].Id));
        var query2 = GithubAssignmentQuery.Build(x => x.WithSubjectCourseId(subjectCourseId2));
        var query3 = GithubAssignmentQuery.Build(x => x.WithBranchName(seedAssignments[2].BranchName));
        var query4 = GithubAssignmentQuery.Build(x => x.WithSubjectCourseOrganizationName(subjectCourseName));

        // Act
        GithubAssignment assignment1 = await repository.QueryAsync(query1, default).SingleAsync();
        GithubAssignment assignment2 = await repository.QueryAsync(query2, default).SingleAsync();
        GithubAssignment assignment3 = await repository.QueryAsync(query3, default).SingleAsync();
        GithubAssignment assignment4 = await repository.QueryAsync(query4, default).SingleAsync();

        // Assert
        assignment1.Should().BeEquivalentTo(seedAssignments[0]);
        assignment2.Should().BeEquivalentTo(seedAssignments[1]);
        assignment3.Should().BeEquivalentTo(seedAssignments[2]);
        assignment4.Should().BeEquivalentTo(seedAssignments[3]);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _fixture.ResetAsync();
    }
}