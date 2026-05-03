using System;
using System.Collections.Generic;

namespace SubtitleQc.Core.Parsers.Internal;

/// <summary>
/// Splits raw subtitle file text into blank-line-separated blocks. Both SRT and WebVTT
/// share this physical layout, so isolating the split logic prevents two parsers from
/// drifting on edge cases like trailing blank lines or CRLF/LF mismatches.
/// </summary>
internal static class SubtitleBlockReader
{
    private static readonly string[] LineSeparators = new[] { "\r\n", "\n", "\r" };

    public static IReadOnlyList<IReadOnlyList<string>> ReadBlocks(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        string[] lines = content.Split(LineSeparators, StringSplitOptions.None);
        List<IReadOnlyList<string>> blocks = new();
        List<string> current = new();
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                FlushIfNotEmpty(current, blocks);
                current = new List<string>();
            }
            else
            {
                current.Add(line);
            }
        }
        FlushIfNotEmpty(current, blocks);
        return blocks;
    }

    private static void FlushIfNotEmpty(List<string> current, List<IReadOnlyList<string>> blocks)
    {
        if (current.Count > 0)
        {
            blocks.Add(current.ToArray());
        }
    }
}
