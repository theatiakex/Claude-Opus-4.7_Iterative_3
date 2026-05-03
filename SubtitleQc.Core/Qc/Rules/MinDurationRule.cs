using System;
using SubtitleQc.Core.Models;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails any cue whose on-screen duration is strictly shorter than the configured
/// minimum. Equality with the threshold passes, matching the industry convention that
/// the boundary value is the lowest permissible legal duration.
/// </summary>
public sealed class MinDurationRule : PerCueRuleBase
{
    public const string RuleName = "MinDuration";

    private readonly TimeSpan _threshold;

    public MinDurationRule(TimeSpan threshold)
    {
        if (threshold < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold cannot be negative.");
        }
        _threshold = threshold;
    }

    public override string Name => RuleName;

    protected override QcResult EvaluateCue(Cue cue)
    {
        if (cue.Duration < _threshold)
        {
            string message = $"Display duration {cue.Duration.TotalSeconds:F3}s is below minimum {_threshold.TotalSeconds:F3}s.";
            return QcResult.Fail(cue.Id, RuleName, message);
        }
        return QcResult.Pass(cue.Id, RuleName);
    }
}
