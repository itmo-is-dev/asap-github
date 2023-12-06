namespace Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Models;

public record struct FirstSubmissionDto(
    Guid SubmissionId,
    Guid UserId,
    Guid AssignmentId);