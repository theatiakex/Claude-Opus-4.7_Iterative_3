using System;
using SubtitleQc.Core.Models;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails any cue whose reading speed (total characters / display seconds) strictly
/// exceeds the configured threshold. Total character count sums all line lengths
/// without inserting separators - line breaks are presentation, not content.
/// </summary>
public sealed class MaxCpsRule : PerCueRuleBase
{
    public const string RuleName = "MaxCps";

    private readonly double _threshold;

    public MaxCpsRule(double threshold)
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
        double seconds = cue.Duration.TotalSeconds;
        if (seconds <= 0)
        {
            return QcResult.Fail(cue.Id, RuleName, "Cue has non-positive duration; reading speed is undefined.");
        }
        int totalChars = CountCharacters(cue);
        double cps = totalChars / seconds;
        if (cps > _threshold)
        {
            string message = $"Reading speed {cps:F2} cps exceeds threshold {_threshold:F2}.";
            return QcResult.Fail(cue.Id, RuleName, message);
        }
        return QcResult.Pass(cue.Id, RuleName);
    }

    private static int CountCharacters(Cue cue)
    {
        int total = 0;
        foreach (string line in cue.Lines)
        {
            total += line.Length;
        }
        return total;
    }
}
