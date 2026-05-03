using System;
using SubtitleQc.Core.Models;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails any cue containing at least one line whose character count strictly exceeds
/// the configured per-line threshold. Boundary equality passes, mirroring industry
/// "max characters per line" conventions.
/// </summary>
public sealed class MaxCplRule : PerCueRuleBase
{
    public const string RuleName = "MaxCpl";

    private readonly int _threshold;

    public MaxCplRule(int threshold)
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
        for (int i = 0; i < cue.Lines.Count; i++)
        {
            string line = cue.Lines[i];
            if (line.Length > _threshold)
            {
                string message = $"Line {i + 1} has {line.Length} characters (threshold {_threshold}).";
                return QcResult.Fail(cue.Id, RuleName, message);
            }
        }
        return QcResult.Pass(cue.Id, RuleName);
    }
}
