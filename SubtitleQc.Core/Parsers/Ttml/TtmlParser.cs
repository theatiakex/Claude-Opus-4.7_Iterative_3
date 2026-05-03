using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Parsers.Abstractions;
using SubtitleQc.Core.Parsers.Internal;

namespace SubtitleQc.Core.Parsers.Ttml;

/// <summary>
/// TTML (Timed Text Markup Language) parser. Each <c>&lt;p&gt;</c> element under the
/// document body is one cue; <c>&lt;br/&gt;</c> children split a cue's text into lines;
/// inline span content is folded into the surrounding line. Element lookup is done by
/// local name so documents declared in any TTML namespace variant (or none) are
/// accepted - the parser only needs to produce the format-agnostic internal model and
/// must not encode wire-format quirks into the QC engine (SRP + DIP).
/// </summary>
public sealed class TtmlParser : ISubtitleParser
{
    private const string ParagraphLocalName = "p";
    private const string LineBreakLocalName = "br";
    private const string BeginAttributeLocalName = "begin";
    private const string EndAttributeLocalName = "end";
    private const string DurationAttributeLocalName = "dur";

    public string FormatName => "TTML";

    public IReadOnlyList<Cue> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        XDocument document = XDocument.Parse(content, LoadOptions.PreserveWhitespace);
        if (document.Root is null)
        {
            return Array.Empty<Cue>();
        }
        List<Cue> cues = new();
        int positional = 0;
        foreach (XElement paragraph in EnumerateParagraphs(document))
        {
            positional++;
            Cue? cue = BuildCue(paragraph, positional);
            if (cue is not null)
            {
                cues.Add(cue);
            }
        }
        return cues;
    }

    private static IEnumerable<XElement> EnumerateParagraphs(XDocument document)
    {
        foreach (XElement element in document.Descendants())
        {
            if (string.Equals(element.Name.LocalName, ParagraphLocalName, StringComparison.Ordinal))
            {
                yield return element;
            }
        }
    }

    private static Cue? BuildCue(XElement paragraph, int positionalIndex)
    {
        (TimeSpan? start, TimeSpan? end) = ExtractTimes(paragraph);
        if (start is null || end is null)
        {
            return null;
        }
        string id = ExtractId(paragraph, positionalIndex);
        IReadOnlyList<string> lines = ExtractLines(paragraph);
        return new Cue(id, start.Value, end.Value, lines);
    }

    private static string ExtractId(XElement paragraph, int positionalIndex)
    {
        XAttribute? idAttribute = FindAttribute(paragraph, "id");
        string? candidate = idAttribute?.Value?.Trim();
        if (!string.IsNullOrEmpty(candidate))
        {
            return candidate!;
        }
        return positionalIndex.ToString(CultureInfo.InvariantCulture);
    }

    private static (TimeSpan? Start, TimeSpan? End) ExtractTimes(XElement paragraph)
    {
        string? beginValue = FindAttribute(paragraph, BeginAttributeLocalName)?.Value;
        string? endValue = FindAttribute(paragraph, EndAttributeLocalName)?.Value;
        string? durValue = FindAttribute(paragraph, DurationAttributeLocalName)?.Value;
        if (string.IsNullOrWhiteSpace(beginValue))
        {
            return (null, null);
        }
        TimeSpan start = TtmlTimeExpressionParser.Parse(beginValue);
        TimeSpan? end = ResolveEnd(start, endValue, durValue);
        return (start, end);
    }

    private static TimeSpan? ResolveEnd(TimeSpan start, string? endValue, string? durValue)
    {
        if (!string.IsNullOrWhiteSpace(endValue))
        {
            return TtmlTimeExpressionParser.Parse(endValue);
        }
        if (!string.IsNullOrWhiteSpace(durValue))
        {
            return start + TtmlTimeExpressionParser.Parse(durValue);
        }
        return null;
    }

    private static XAttribute? FindAttribute(XElement element, string localName)
    {
        foreach (XAttribute attribute in element.Attributes())
        {
            if (string.Equals(attribute.Name.LocalName, localName, StringComparison.Ordinal))
            {
                return attribute;
            }
        }
        return null;
    }

    private static IReadOnlyList<string> ExtractLines(XElement paragraph)
    {
        List<string> lines = new();
        StringBuilder current = new();
        foreach (XNode node in paragraph.Nodes())
        {
            WalkInto(node, lines, current);
        }
        FlushLine(lines, current);
        return lines;
    }

    private static void WalkInto(XNode node, List<string> lines, StringBuilder current)
    {
        if (node is XText text)
        {
            current.Append(text.Value);
            return;
        }
        if (node is XElement element)
        {
            WalkElement(element, lines, current);
        }
    }

    private static void WalkElement(XElement element, List<string> lines, StringBuilder current)
    {
        if (string.Equals(element.Name.LocalName, LineBreakLocalName, StringComparison.Ordinal))
        {
            FlushLine(lines, current);
            return;
        }
        foreach (XNode child in element.Nodes())
        {
            WalkInto(child, lines, current);
        }
    }

    private static void FlushLine(List<string> lines, StringBuilder current)
    {
        lines.Add(current.ToString());
        current.Clear();
    }
}
