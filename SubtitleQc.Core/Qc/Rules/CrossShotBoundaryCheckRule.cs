using System;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails any cue that visibly straddles a video cut: a cut at time <c>T</c> falls
/// strictly inside the cue's display interval (<c>cue.Start &lt; T &lt; cue.End</c>).
/// Strict inequality on both sides matches the editorial convention that a cue
/// ending or starting exactly on a cut is correctly aligned, not a violation.
/// External cut data is injected via <see cref="IShotChangeProvider"/>, keeping the
/// rule oblivious to how that data was sourced (DIP).
/// </summary>
public sealed class CrossShotBoundaryCheckRule : PerCueRuleBase
{
    public const string RuleName = "CrossShotBoundaryCheck";

    private readonly IShotChangeProvider _shotChangeProvider;

    public CrossShotBoundaryCheckRule(IShotChangeProvider shotChangeProvider)
    {
        ArgumentNullException.ThrowIfNull(shotChangeProvider);
        _shotChangeProvider = shotChangeProvider;
    }

    public override string Name => RuleName;

    protected override QcResult EvaluateCue(Cue cue)
    {
        foreach (TimeSpan cut in _shotChangeProvider.GetShotChangeTimestamps())
        {
            if (CutFallsInsideCue(cut, cue))
            {
                string message = $"Cue spans shot change at {cut}.";
                return QcResult.Fail(cue.Id, RuleName, message);
            }
        }
        return QcResult.Pass(cue.Id, RuleName);
    }

    private static bool CutFallsInsideCue(TimeSpan cut, Cue cue)
    {
        return cut > cue.Start && cut < cue.End;
    }
}
