using System.Collections.Generic;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Cross-cue invariant: a cue is failed when any other cue that started earlier in
/// time is still on-screen at this cue's start. The later cue is the one "blamed"
/// because correctness is established by the natural reading order of the timeline.
/// Adjacent cues (one ending exactly when the next starts) do not overlap.
/// </summary>
public sealed class OverlapCheckRule : IQcRule
{
    public const string RuleName = "OverlapCheck";

    public string Name => RuleName;

    public IEnumerable<QcResult> Evaluate(IReadOnlyList<Cue> cues)
    {
        foreach (Cue current in cues)
        {
            yield return EvaluateOne(current, cues);
        }
    }

    private static QcResult EvaluateOne(Cue current, IReadOnlyList<Cue> all)
    {
        foreach (Cue other in all)
        {
            if (ReferenceEquals(other, current))
            {
                continue;
            }
            if (Overlaps(earlier: other, later: current))
            {
                string message = $"Overlaps with cue {other.Id} (ends at {other.End}).";
                return QcResult.Fail(current.Id, RuleName, message);
            }
        }
        return QcResult.Pass(current.Id, RuleName);
    }

    private static bool Overlaps(Cue earlier, Cue later)
    {
        return earlier.Start < later.Start && earlier.End > later.Start;
    }
}
