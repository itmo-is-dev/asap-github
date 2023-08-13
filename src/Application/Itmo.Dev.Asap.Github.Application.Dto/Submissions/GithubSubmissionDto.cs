namespace Itmo.Dev.Asap.Github.Application.Dto.Submissions;

public record GithubSubmissionDto(Guid Id, string Organization, string Repository, long PrNumber);