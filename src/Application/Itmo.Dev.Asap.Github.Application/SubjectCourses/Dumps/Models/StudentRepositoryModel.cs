using Itmo.Dev.Asap.Github.Application.Abstractions.Octokit.Models;

namespace Itmo.Dev.Asap.Github.Application.SubjectCourses.Dumps.Models;

public record struct StudentRepositoryModel(Guid StudentId, GithubRepositoryModel Repository);