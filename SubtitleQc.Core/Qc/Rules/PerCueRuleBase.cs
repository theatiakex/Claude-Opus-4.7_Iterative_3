using System.Collections.Generic;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Convenience base for rules whose verdict for a cue depends only on that cue. Keeps
/// the iteration boilerplate out of concrete rules so each subclass focuses on a single
/// invariant (SRP).
/// </summary>
public abstract class PerCueRuleBase : IQcRule
{
    public abstract string Name { get; }

    public IEnumerable<QcResult> Evaluate(IReadOnlyList<Cue> cues)
    {
        foreach (Cue cue in cues)
        {
            yield return EvaluateCue(cue);
        }
    }

    protected abstract QcResult EvaluateCue(Cue cue);
}
