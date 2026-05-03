using System;

namespace SubtitleQc.Core.Qc;

/// <summary>
/// Structured, JSON-serializable record of a single rule's verdict against a single cue.
/// The combination of CueId + RuleName uniquely identifies the assertion and allows
/// reports to be re-aggregated downstream without losing traceability.
/// </summary>
public sealed class QcResult
{
    public string CueId { get; }

    public string RuleName { get; }

    public QcStatus Status { get; }

    public string? Message { get; }

    public QcResult(string cueId, string ruleName, QcStatus status, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(cueId);
        ArgumentNullException.ThrowIfNull(ruleName);
        CueId = cueId;
        RuleName = ruleName;
        Status = status;
        Message = message;
    }

    public static QcResult Pass(string cueId, string ruleName)
        => new(cueId, ruleName, QcStatus.Passed);

    public static QcResult Fail(string cueId, string ruleName, string message)
        => new(cueId, ruleName, QcStatus.Failed, message);
}
