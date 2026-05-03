using System.Collections.Generic;
using SubtitleQc.Core.Models;

namespace SubtitleQc.Core.Qc.Abstractions;

/// <summary>
/// Contract for any rule the QC engine can run. Rules receive the entire ordered cue
/// collection (rather than a single cue) so that cross-cue invariants such as overlap
/// detection can live behind the same abstraction as per-cue checks. Returning a flat
/// result list keeps the engine's job purely orchestrative (DIP).
/// </summary>
public interface IQcRule
{
    /// <summary>Stable identifier used for traceability in QcResult.RuleName.</summary>
    string Name { get; }

    IEnumerable<QcResult> Evaluate(IReadOnlyList<Cue> cues);
}
