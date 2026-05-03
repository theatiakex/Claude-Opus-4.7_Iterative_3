using System.Collections.Generic;
using SubtitleQc.Core.Models;

namespace SubtitleQc.Core.Parsers.Abstractions;

/// <summary>
/// Contract for any concrete subtitle format parser. Producing only the format-agnostic
/// internal model is what guarantees the QC engine never gains knowledge of any wire
/// format - new formats slot in via new <see cref="ISubtitleParser"/> implementations
/// without touching engine or rule code (OCP + DIP).
/// </summary>
public interface ISubtitleParser
{
    /// <summary>Stable name of the format this parser handles (e.g. "SRT", "WebVTT").</summary>
    string FormatName { get; }

    IReadOnlyList<Cue> Parse(string content);
}
