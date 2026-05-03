using System;
using System.Collections.Generic;
using System.Linq;

namespace SubtitleQc.Core.Models;

/// <summary>
/// Format-agnostic representation of a single subtitle cue. Acts as the canonical
/// internal data contract that all parsers must produce and the QC engine consumes.
/// Decoupling this type from any wire format is what enables the engine to remain
/// closed for modification while open to new input formats (OCP).
/// </summary>
public sealed class Cue
{
    public string Id { get; }

    public TimeSpan Start { get; }

    public TimeSpan End { get; }

    public IReadOnlyList<string> Lines { get; }

    /// <summary>
    /// Optional frame-accurate start position when the source format supplies it (e.g.
    /// EBU-STL or TTML with <c>tickRate</c>). Kept nullable so the iteration-1 ctor
    /// stays backwards-compatible and rules that need frames can opt in via this
    /// property without forcing every parser to compute frames it does not have.
    /// </summary>
    public int? StartFrame { get; }

    public Cue(
        string id,
        TimeSpan start,
        TimeSpan end,
        IReadOnlyList<string> lines,
        int? startFrame = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(lines);
        if (end < start)
        {
            throw new ArgumentException("Cue end must be greater than or equal to start.", nameof(end));
        }

        Id = id;
        Start = start;
        End = end;
        Lines = lines.ToArray();
        StartFrame = startFrame;
    }

    /// <summary>
    /// Convenience accessor for the cue's display duration. Kept on the model itself
    /// so rules don't repeatedly re-derive a basic geometric property.
    /// </summary>
    public TimeSpan Duration => End - Start;
}
