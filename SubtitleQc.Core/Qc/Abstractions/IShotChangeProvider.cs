using System;
using System.Collections.Generic;

namespace SubtitleQc.Core.Qc.Abstractions;

/// <summary>
/// Source of external shot-change (visual cut) data, kept behind an abstraction so the
/// QC engine never depends on how that data is sourced (file, REST API, message queue).
/// Both time-based and frame-based accessors are exposed because different shot-detection
/// pipelines emit one or the other natively, and converting between the two requires
/// frame-rate metadata that is intentionally outside this contract's responsibility.
/// Returning empty lists is the well-defined "no cuts known" signal - implementations
/// must never return null.
/// </summary>
public interface IShotChangeProvider
{
    IReadOnlyList<TimeSpan> GetShotChangeTimestamps();

    IReadOnlyList<int> GetShotChangeFrames();
}
