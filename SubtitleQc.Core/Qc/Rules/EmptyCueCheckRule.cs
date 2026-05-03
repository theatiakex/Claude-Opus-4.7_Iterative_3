using SubtitleQc.Core.Models;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails cues whose lines are collectively devoid of visible content (only whitespace,
/// tabs, or empty strings). A cue stripped to whitespace conveys no information to the
/// viewer and is therefore a defect regardless of its timing.
/// </summary>
public sealed class EmptyCueCheckRule : PerCueRuleBase
{
    public const string RuleName = "EmptyCueCheck";

    public override string Name => RuleName;

    protected override QcResult EvaluateCue(Cue cue)
    {
        if (IsEffectivelyEmpty(cue))
        {
            return QcResult.Fail(cue.Id, RuleName, "Cue contains no visible text.");
        }
        return QcResult.Pass(cue.Id, RuleName);
    }

    private static bool IsEffectivelyEmpty(Cue cue)
    {
        foreach (string line in cue.Lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                return false;
            }
        }
        return true;
    }
}
