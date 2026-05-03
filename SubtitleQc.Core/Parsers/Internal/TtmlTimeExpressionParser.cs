using System;
using System.Globalization;

namespace SubtitleQc.Core.Parsers.Internal;

/// <summary>
/// Decodes a TTML time expression (W3C TTML2 §10.3.1). TTML supports two distinct
/// grammars on the same attributes: <c>clock-time</c> (HH:MM:SS[.fraction]) and
/// <c>offset-time</c> (a metric quantity like <c>5s</c>, <c>1500ms</c>, <c>2.5h</c>).
/// SRT/WebVTT only use clock-time, so this richer grammar lives in a dedicated helper
/// rather than expanding <see cref="TimeCodeParser"/> and risking SRP creep.
/// Frame ('f') and tick ('t') metrics are intentionally unsupported - they require
/// frame-rate metadata that TTML carries on its root element and is out of scope here.
/// </summary>
internal static class TtmlTimeExpressionParser
{
    public static TimeSpan Parse(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        string trimmed = expression.Trim();
        if (trimmed.Length == 0)
        {
            throw new FormatException("Empty TTML time expression.");
        }
        if (IsClockTime(trimmed))
        {
            return TimeCodeParser.Parse(trimmed);
        }
        return ParseOffsetTime(trimmed);
    }

    private static bool IsClockTime(string token)
    {
        return token.IndexOf(':') >= 0;
    }

    private static TimeSpan ParseOffsetTime(string token)
    {
        (string numericPart, string unit) = SplitMetric(token);
        if (!double.TryParse(numericPart, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            throw new FormatException($"Invalid TTML offset-time value: '{token}'.");
        }
        return ConvertToTimeSpan(value, unit, token);
    }

    private static (string Numeric, string Unit) SplitMetric(string token)
    {
        int splitAt = FindUnitStart(token);
        if (splitAt < 0 || splitAt == 0)
        {
            throw new FormatException($"Missing metric unit in TTML offset-time: '{token}'.");
        }
        return (token[..splitAt], token[splitAt..].ToLowerInvariant());
    }

    private static int FindUnitStart(string token)
    {
        for (int i = 0; i < token.Length; i++)
        {
            char c = token[i];
            if (!IsNumericChar(c))
            {
                return i;
            }
        }
        return -1;
    }

    private static bool IsNumericChar(char c)
    {
        return char.IsDigit(c) || c == '.' || c == '+' || c == '-';
    }

    private static TimeSpan ConvertToTimeSpan(double value, string unit, string original)
    {
        return unit switch
        {
            "h" => TimeSpan.FromHours(value),
            "m" => TimeSpan.FromMinutes(value),
            "s" => TimeSpan.FromSeconds(value),
            "ms" => TimeSpan.FromMilliseconds(value),
            _ => throw new FormatException($"Unsupported TTML time metric '{unit}' in '{original}'."),
        };
    }
}
