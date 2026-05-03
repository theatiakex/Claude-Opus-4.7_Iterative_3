using System;
using SubtitleQc.Core.Models;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails any cue whose number of lines strictly exceeds the configured threshold. A cue
/// at exactly the threshold passes - this matches the conventional reading of a "max"
/// constraint as inclusive of the boundary value.
/// </summary>
public sealed class MaxLinesRule : PerCueRuleBase
{
    public const string RuleName = "MaxLines";

    private readonly int _threshold;

    public MaxLinesRule(int threshold)
    {
        if (threshold < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold cannot be negative.");
        }
        _threshold = threshold;
    }

    public override string Name => RuleName;

    protected override QcResult EvaluateCue(Cue cue)
    {
        if (cue.Lines.Count > _threshold)
        {
            string message = $"Cue has {cue.Lines.Count} lines (threshold {_threshold}).";
            return QcResult.Fail(cue.Id, RuleName, message);
        }
        return QcResult.Pass(cue.Id, RuleName);
    }
}
