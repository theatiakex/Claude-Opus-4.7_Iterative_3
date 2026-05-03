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

    public Cue(string id, TimeSpan start, TimeSpan end, IReadOnlyList<string> lines)
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
    }

    /// <summary>
    /// Convenience accessor for the cue's display duration. Kept on the model itself
    /// so rules don't repeatedly re-derive a basic geometric property.
    /// </summary>
    public TimeSpan Duration => End - Start;
}
