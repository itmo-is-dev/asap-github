using Itmo.Dev.Asap.Github.Commands.Models;
using Itmo.Dev.Asap.Github.Commands.SubmissionCommands;

namespace Itmo.Dev.Asap.Github.Commands.CommandVisitors;

public interface ISubmissionCommandVisitor
{
    Task<SubmissionCommandResult> VisitAsync(ActivateCommand command);

    Task<SubmissionCommandResult> VisitAsync(BanCommand command);

    Task<SubmissionCommandResult> VisitAsync(CreateSubmissionCommand command);

    Task<SubmissionCommandResult> VisitAsync(DeactivateCommand command);

    Task<SubmissionCommandResult> VisitAsync(DeleteCommand command);

    Task<SubmissionCommandResult> VisitAsync(HelpCommand command);

    Task<SubmissionCommandResult> VisitAsync(MarkReviewedCommand command);

    Task<SubmissionCommandResult> VisitAsync(RateCommand command);

    Task<SubmissionCommandResult> VisitAsync(UpdateCommand command);
}