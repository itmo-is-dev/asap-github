using Itmo.Dev.Asap.Github.Domain.Users;

namespace Itmo.Dev.Asap.Github.Domain.SubjectCourses;

public record GithubSubjectCourseStudent(Guid SubjectCourseId, GithubUser User, long RepositoryId);