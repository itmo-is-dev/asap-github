using Bogus;
using Itmo.Dev.Asap.Github.Application.Models.Assignments;
using Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Models.Users;
using Itmo.Dev.Asap.Github.Tests.Models;

namespace Itmo.Dev.Asap.Github.Tests.Extensions;

public static class FakerExtensions
{
    public static GithubAssignment GithubAssignment(this Faker faker, Guid? subjectCourseId = null)
    {
        return new GithubAssignment(
            Id: faker.Random.Guid(),
            SubjectCourseId: subjectCourseId ?? faker.Random.Guid(),
            BranchName: $"lab-{faker.Random.Int(min: 0)}",
            RepositoryPath: string.Empty);
    }

    public static GithubSubjectCourseModel GithubSubjectCourseModel(this Faker faker, Guid? id = null)
    {
        return new GithubSubjectCourseModel(
            id ?? faker.Random.Guid(),
            OrganizationId: faker.Random.Long(1000, 2000),
            TemplateRepositoryId: faker.Random.Long(1000, 2000),
            MentorTeamId: faker.Random.Long(1000, 2000));
    }

    public static GithubSubjectCourse GithubSubjectCourse(
        this Faker faker,
        Guid? id = null,
        long? organizationId = null,
        long? templateRepositoryId = null,
        long? mentorTeamId = null)
    {
        return new GithubSubjectCourse(
            id ?? faker.Random.Guid(),
            organizationId ?? faker.Random.Long(1000, 2000),
            templateRepositoryId ?? faker.Random.Long(1000, 2000),
            mentorTeamId ?? faker.Random.Long(1000, 2000));
    }

    public static GithubUser GithubUser(this Faker faker)
    {
        return new GithubUser(faker.Random.Guid(), faker.Random.Long(1000, 2000));
    }
}