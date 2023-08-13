using Bogus;
using Itmo.Dev.Asap.Github.Domain.Assignments;
using Itmo.Dev.Asap.Github.Domain.SubjectCourses;
using Itmo.Dev.Asap.Github.Domain.Users;
using Itmo.Dev.Asap.Github.Tests.Models;

namespace Itmo.Dev.Asap.Github.Tests.Extensions;

public static class FakerExtensions
{
    public static GithubAssignment GithubAssignment(this Faker faker, Guid? subjectCourseId = null)
    {
        return new GithubAssignment(
            faker.Random.Guid(),
            subjectCourseId ?? faker.Random.Guid(),
            $"lab-{faker.Random.Int(min: 0)}");
    }

    public static GithubSubjectCourseModel GithubSubjectCourseModel(this Faker faker, Guid? id = null)
    {
        return new GithubSubjectCourseModel(
            id ?? faker.Random.Guid(),
            faker.Company.CompanyName(),
            faker.Commerce.ProductName(),
            faker.Company.CompanySuffix());
    }

    public static GithubSubjectCourse GithubSubjectCourse(
        this Faker faker,
        Guid? id = null,
        string? organizationName = null,
        string? templateRepositoryName = null,
        string? mentorTeamName = null)
    {
        return new GithubSubjectCourse(
            id ?? faker.Random.Guid(),
            organizationName ?? faker.Company.CompanyName(),
            templateRepositoryName ?? faker.Commerce.ProductName(),
            mentorTeamName ?? faker.Commerce.ProductName());
    }

    public static GithubUser GithubUser(this Faker faker)
    {
        return new GithubUser(faker.Random.Guid(), faker.Internet.UserName());
    }
}