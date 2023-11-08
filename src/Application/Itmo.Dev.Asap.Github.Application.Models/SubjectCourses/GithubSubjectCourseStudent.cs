using Itmo.Dev.Asap.Github.Application.Models.Users;

namespace Itmo.Dev.Asap.Github.Application.Models.SubjectCourses;

public record GithubSubjectCourseStudent(Guid SubjectCourseId, GithubUser User, long RepositoryId);