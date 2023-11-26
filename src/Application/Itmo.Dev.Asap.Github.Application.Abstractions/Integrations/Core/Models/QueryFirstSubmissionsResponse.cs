namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;

public record QueryFirstSubmissionsResponse(
    IEnumerable<FirstSubmissionDto> Submissions,
    string? PageToken);