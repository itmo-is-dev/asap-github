namespace Itmo.Dev.Asap.Github.Application.Models.PullRequests;

public record PullRequestDto(
    long SenderId,
    string SenderUsername,
    string Payload,
    long OrganizationId,
    string OrganizationName,
    long RepositoryId,
    string RepositoryName,
    string BranchName,
    long PullRequestId)
{
    public override string ToString()
    {
        return $"{Payload} with branch {BranchName} from {SenderUsername}";
    }
}