using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Commands;
using Itmo.Dev.Asap.Github.Application.Contracts.Submissions.Models;

namespace Itmo.Dev.Asap.Github.Application.Contracts.Submissions.CommandVisitors;

public interface ISubmissionCommandVisitor
{
    Task<SubmissionCommandResult> VisitAsync(ActivateCommand command);

    Task<SubmissionCommandResult> VisitAsync(BanCommand command);

    Task<SubmissionCommandResult> VisitAsync(UnbanCommand command);

    Task<SubmissionCommandResult> VisitAsync(CreateSubmissionCommand command);

    Task<SubmissionCommandResult> VisitAsync(DeactivateCommand command);

    Task<SubmissionCommandResult> VisitAsync(DeleteCommand command);

    Task<SubmissionCommandResult> VisitAsync(HelpCommand command);

    Task<SubmissionCommandResult> VisitAsync(MarkReviewedCommand command);

    Task<SubmissionCommandResult> VisitAsync(RateCommand command);

    Task<SubmissionCommandResult> VisitAsync(UpdateCommand command);
}