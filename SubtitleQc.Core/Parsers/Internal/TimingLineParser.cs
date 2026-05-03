using System;

namespace SubtitleQc.Core.Parsers.Internal;

/// <summary>
/// Splits an SRT/WebVTT timing line ("HH:MM:SS,fff --> HH:MM:SS,fff [settings]") into
/// its start/end timecodes. Trailing settings (positioning hints in WebVTT) are
/// intentionally discarded - they do not affect QC at this iteration.
/// </summary>
internal static class TimingLineParser
{
    private const string Arrow = "-->";

    public static (TimeSpan Start, TimeSpan End) Parse(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        int arrowIndex = line.IndexOf(Arrow, StringComparison.Ordinal);
        if (arrowIndex < 0)
        {
            throw new FormatException($"Timing line missing '-->': '{line}'.");
        }
        string startToken = line[..arrowIndex].Trim();
        string remainder = line[(arrowIndex + Arrow.Length)..].Trim();
        string endToken = ExtractEndToken(remainder);
        TimeSpan start = TimeCodeParser.Parse(startToken);
        TimeSpan end = TimeCodeParser.Parse(endToken);
        return (start, end);
    }

    private static string ExtractEndToken(string remainder)
    {
        int firstSpace = remainder.IndexOf(' ');
        return firstSpace < 0 ? remainder : remainder[..firstSpace];
    }
}
