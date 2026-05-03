using System;
using System.Globalization;

namespace SubtitleQc.Core.Parsers.Internal;

/// <summary>
/// Decodes a single HH:MM:SS{,|.}fff timecode token. SRT uses ',' as the millisecond
/// separator, WebVTT uses '.'. Centralising the parse keeps both parsers free of
/// duplicated string-munging and makes future timecode quirks (e.g. negative offsets)
/// a one-place change.
/// </summary>
internal static class TimeCodeParser
{
    private const string TimeFormat = @"hh\:mm\:ss\.fff";

    public static TimeSpan Parse(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        string normalized = NormalizeMillisecondSeparator(token.Trim());
        if (TimeSpan.TryParseExact(normalized, TimeFormat, CultureInfo.InvariantCulture, out TimeSpan exact))
        {
            return exact;
        }
        if (TimeSpan.TryParse(normalized, CultureInfo.InvariantCulture, out TimeSpan loose))
        {
            return loose;
        }
        throw new FormatException($"Unrecognised timecode token: '{token}'.");
    }

    private static string NormalizeMillisecondSeparator(string token)
    {
        return token.Replace(',', '.');
    }
}
