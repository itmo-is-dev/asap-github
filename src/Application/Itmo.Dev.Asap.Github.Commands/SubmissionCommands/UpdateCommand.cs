using CommandLine;
using Itmo.Dev.Asap.Github.Commands.CommandVisitors;
using Itmo.Dev.Asap.Github.Commands.Models;
using Itmo.Dev.Asap.Github.Common.Tools;

namespace Itmo.Dev.Asap.Github.Commands.SubmissionCommands;

[Verb("/update")]
public class UpdateCommand : ISubmissionCommand
{
    public UpdateCommand(int? submissionCode, double? ratingPercent, double? extraPoints, string? dateStr)
    {
        SubmissionCode = submissionCode;
        RatingPercent = ratingPercent;
        ExtraPoints = extraPoints;
        DateStr = dateStr;
    }

    [Value(0, Required = false, MetaName = "SubmissionCode")]
    public int? SubmissionCode { get; }

    [Option('r', "rating", Group = "update", Required = false)]
    public double? RatingPercent { get; }

    [Option('e', "extra", Group = "update", Required = false)]
    public double? ExtraPoints { get; }

    [Option('d', "date", Group = "update", Required = false)]
    public string? DateStr { get; }

    public string Name => "/update";

    public Task<SubmissionCommandResult> AcceptAsync(ISubmissionCommandVisitor visitor)
    {
        return visitor.VisitAsync(this);
    }

    public DateOnly? GetDate()
    {
        return string.IsNullOrEmpty(DateStr) ? null : RuCultureDate.Parse(DateStr);
    }

    public override string ToString()
    {
        return $" {{ SubmissionCode : {SubmissionCode}," +
               $" RatingPercent: {RatingPercent}" +
               $" ExtraPoints: {ExtraPoints}" +
               $" DateStr: {DateStr}" +
               " }";
    }
}