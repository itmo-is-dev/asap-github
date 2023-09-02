using Dapper;
using FluentAssertions;
using Itmo.Dev.Asap.Github.Application.DataAccess.Queries;
using Itmo.Dev.Asap.Github.DataAccess.Repositories;
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

namespace Itmo.Dev.Asap.Github.Tests.Repositories;

[Collection(nameof(DatabaseCollectionFixture))]
public class GithubSubjectCourseRepositoryTests : TestBase, IAsyncLifetime
{
    private readonly GithubDatabaseFixture _fixture;

    public GithubSubjectCourseRepositoryTests(GithubDatabaseFixture fixture)
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
        var repository = new GithubSubjectCourseRepository(unit, connectionProvider);
        GithubSubjectCourse subjectCourse = Faker.GithubSubjectCourse();

        // Act
        repository.Add(subjectCourse);
        await unit.CommitAsync(IsolationLevel.ReadCommitted, default);

        // Assert
        const string sql =
            """
            select subject_course_id as Id,
            subject_course_organization_id as OrganizationId,
            subject_course_template_repository_id as TemplateRepositoryId,
            subject_course_mentor_team_id as MentorTeamId
            from subject_courses
            """;

        GithubSubjectCourseModel[] subjectCourses = _fixture.Connection.Query<GithubSubjectCourseModel>(sql).ToArray();

        GithubSubjectCourseModel subjectCourseModel = subjectCourses.Should().ContainSingle().Subject;
        subjectCourseModel.Should().NotBeEquivalentTo(subjectCourse);
    }

    [Fact]
    public async Task Update_ShouldUpdateDatabaseRecordCorrectly()
    {
        // Arrange
        await using PostgresConnectionProviderMock connectionProvider = _fixture.CreateConnectionProvider();
        ILogger<ReusableUnitOfWork> logger = _fixture.CreateLogger<ReusableUnitOfWork>();

        using var unit = new ReusableUnitOfWork(connectionProvider, logger);
        var repository = new GithubSubjectCourseRepository(unit, connectionProvider);
        GithubSubjectCourse subjectCourse = Faker.GithubSubjectCourse();

        repository.Add(subjectCourse);
        await unit.CommitAsync(IsolationLevel.ReadCommitted, default);

        subjectCourse.MentorTeamId = Faker.Random.Long(1000, 2000);

        // Act
        repository.Update(subjectCourse);
        await unit.CommitAsync(IsolationLevel.ReadCommitted, default);

        // Assert
        const string sql =
            """
            select subject_course_id as Id,
            subject_course_organization_id as OrganizationId,
            subject_course_template_repository_id as TemplateRepositoryId,
            subject_course_mentor_team_id as MentorTeamId
            from subject_courses
            """;

        GithubSubjectCourseModel[] subjectCourses = _fixture.Connection.Query<GithubSubjectCourseModel>(sql).ToArray();

        GithubSubjectCourseModel subjectCourseModel = subjectCourses.Should().ContainSingle().Subject;
        subjectCourseModel.Should().NotBeEquivalentTo(subjectCourse);
    }

    [Fact]
    public async Task QueryAsync_ShouldReturnCorrectRecords()
    {
        // Arrange
        await using PostgresConnectionProviderMock connectionProvider = _fixture.CreateConnectionProvider();
        ILogger<ReusableUnitOfWork> logger = _fixture.CreateLogger<ReusableUnitOfWork>();

        using var unit = new ReusableUnitOfWork(connectionProvider, logger);
        var repository = new GithubSubjectCourseRepository(unit, connectionProvider);

        GithubSubjectCourse[] courses =
        {
            Faker.GithubSubjectCourse(),
            Faker.GithubSubjectCourse(),
            Faker.GithubSubjectCourse(),
            Faker.GithubSubjectCourse(),
        };

        foreach (GithubSubjectCourse course in courses)
        {
            repository.Add(course);
        }

        await unit.CommitAsync(IsolationLevel.ReadCommitted, default);

        var query1 = GithubSubjectCourseQuery.Build(x => x.WithId(courses[0].Id));
        var query2 = GithubSubjectCourseQuery.Build(x => x.WithOrganizationId(courses[1].OrganizationId));
        var query3 = GithubSubjectCourseQuery.Build(x => x.WithTemplateRepositoryId(courses[2].TemplateRepositoryId));
        var query4 = GithubSubjectCourseQuery.Build(x => x.WithMentorTeamId(courses[3].MentorTeamId));

        // Act
        GithubSubjectCourse course1 = await repository.QueryAsync(query1, default).SingleAsync();
        GithubSubjectCourse course2 = await repository.QueryAsync(query2, default).SingleAsync();
        GithubSubjectCourse course3 = await repository.QueryAsync(query3, default).SingleAsync();
        GithubSubjectCourse course4 = await repository.QueryAsync(query4, default).SingleAsync();

        // Assert
        course1.Should().BeEquivalentTo(courses[0]);
        course2.Should().BeEquivalentTo(courses[1]);
        course3.Should().BeEquivalentTo(courses[2]);
        course4.Should().BeEquivalentTo(courses[3]);
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