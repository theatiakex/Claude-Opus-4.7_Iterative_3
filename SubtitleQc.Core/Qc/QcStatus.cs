namespace SubtitleQc.Core.Qc;

/// <summary>
/// Outcome of a single rule evaluation against a cue. Kept as an explicit enum (rather
/// than a boolean) so future iterations can introduce additional terminal states such
/// as Warning or NotApplicable without breaking existing consumers.
/// </summary>
public enum QcStatus
{
    Passed = 0,
    Failed = 1,
}
