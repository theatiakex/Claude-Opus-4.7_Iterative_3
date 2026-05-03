using System;
using System.Collections.Generic;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Parsers.Abstractions;
using SubtitleQc.Core.Parsers.Internal;

namespace SubtitleQc.Core.Parsers.WebVtt;

/// <summary>
/// WebVTT parser. A WebVTT file begins with the "WEBVTT" signature followed by blank-
/// line separated cue blocks. A cue block may contain an optional identifier line
/// preceding its timing line. NOTE/STYLE/REGION blocks (no timing line) are skipped.
/// </summary>
public sealed class WebVttParser : ISubtitleParser
{
    private const string Signature = "WEBVTT";

    public string FormatName => "WebVTT";

    public IReadOnlyList<Cue> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        IReadOnlyList<IReadOnlyList<string>> blocks = SubtitleBlockReader.ReadBlocks(content);
        List<Cue> cues = new();
        for (int i = 0; i < blocks.Count; i++)
        {
            if (i == 0 && IsSignatureBlock(blocks[i]))
            {
                continue;
            }
            Cue? cue = TryParseCueBlock(blocks[i], fallbackIndex: cues.Count + 1);
            if (cue is not null)
            {
                cues.Add(cue);
            }
        }
        return cues;
    }

    private static bool IsSignatureBlock(IReadOnlyList<string> block)
    {
        return block.Count > 0 && block[0].StartsWith(Signature, StringComparison.Ordinal);
    }

    private static Cue? TryParseCueBlock(IReadOnlyList<string> block, int fallbackIndex)
    {
        int timingLineIndex = LocateTimingLine(block);
        if (timingLineIndex < 0)
        {
            return null;
        }
        string id = ExtractId(block, timingLineIndex, fallbackIndex);
        (TimeSpan start, TimeSpan end) = TimingLineParser.Parse(block[timingLineIndex]);
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
