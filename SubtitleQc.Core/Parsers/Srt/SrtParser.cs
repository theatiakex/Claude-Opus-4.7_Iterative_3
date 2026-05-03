using System.Collections.Generic;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Parsers.Abstractions;
using SubtitleQc.Core.Parsers.Internal;

namespace SubtitleQc.Core.Parsers.Srt;

/// <summary>
/// SRT format parser. An SRT block is: an integer index line, a timing line, and one
/// or more text lines. The numeric index is preserved as <see cref="Cue.Id"/> when
/// present; otherwise a synthetic positional id is assigned. The parser produces only
/// the internal model and never references QC types (SRP).
/// </summary>
public sealed class SrtParser : ISubtitleParser
{
    public string FormatName => "SRT";

    public IReadOnlyList<Cue> Parse(string content)
    {
        IReadOnlyList<IReadOnlyList<string>> blocks = SubtitleBlockReader.ReadBlocks(content);
        List<Cue> cues = new();
        for (int i = 0; i < blocks.Count; i++)
        {
            Cue? cue = TryParseBlock(blocks[i], fallbackIndex: i + 1);
            if (cue is not null)
            {
                cues.Add(cue);
            }
        }
        return cues;
    }

    private static Cue? TryParseBlock(IReadOnlyList<string> block, int fallbackIndex)
    {
        if (block.Count == 0)
        {
            return null;
        }
        int timingLineIndex = LocateTimingLine(block);
        if (timingLineIndex < 0)
        {
            return null;
        }
        string id = ExtractId(block, timingLineIndex, fallbackIndex);
        (System.TimeSpan start, System.TimeSpan end) = TimingLineParser.Parse(block[timingLineIndex]);
        IReadOnlyList<string> lines = ExtractTextLines(block, timingLineIndex);
        return new Cue(id, start, end, lines);
    }

    private static int LocateTimingLine(IReadOnlyList<string> block)
    {
        for (int i = 0; i < block.Count; i++)
        {
            if (block[i].Contains("-->"))
            {
                return i;
            }
        }
        return -1;
    }

    private static string ExtractId(IReadOnlyList<string> block, int timingLineIndex, int fallbackIndex)
    {
        if (timingLineIndex > 0)
        {
            string candidate = block[0].Trim();
            if (candidate.Length > 0)
            {
                return candidate;
            }
        }
        return fallbackIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static IReadOnlyList<string> ExtractTextLines(IReadOnlyList<string> block, int timingLineIndex)
    {
        List<string> textLines = new();
        for (int i = timingLineIndex + 1; i < block.Count; i++)
        {
            textLines.Add(block[i]);
        }
        return textLines;
    }
}
