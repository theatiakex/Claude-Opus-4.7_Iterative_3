using System;
using System.Collections.Generic;
using System.Linq;

namespace SubtitleQc.Core.Qc;

/// <summary>
/// Aggregated, JSON-serializable output of a full QC pass. Holds the flat list of
/// (cue, rule) results so the data is amenable to grouping/filtering by either axis
/// without imposing a fixed shape on the consumer.
/// </summary>
public sealed class QcReport
{
    public IReadOnlyList<QcResult> Results { get; }

    public QcReport(IEnumerable<QcResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        Results = results.ToArray();
    }

    public bool HasFailures => Results.Any(r => r.Status == QcStatus.Failed);
}
