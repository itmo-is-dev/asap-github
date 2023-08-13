using CommandLine;
using Itmo.Dev.Asap.Github.Commands.CommandVisitors;
using Itmo.Dev.Asap.Github.Commands.Models;

namespace Itmo.Dev.Asap.Github.Commands.SubmissionCommands;

[Verb("/rate")]
public class RateCommand : ISubmissionCommand
{
    public RateCommand(double ratingPercent, double? extraPoints)
    {
        RatingPercent = ratingPercent;
        ExtraPoints = extraPoints;
    }

    [Value(0, Required = true, MetaName = "RatingPercent")]
    public double RatingPercent { get; }

    [Value(1, Required = false, Default = 0.0, MetaName = "ExtraPoints")]
    public double? ExtraPoints { get; }

    public string Name => "/rate";

    public Task<SubmissionCommandResult> AcceptAsync(ISubmissionCommandVisitor visitor)
    {
        return visitor.VisitAsync(this);
    }

    public override string ToString()
        => $"RatingPercent: {RatingPercent}, ExtraPoints: {ExtraPoints}";
}