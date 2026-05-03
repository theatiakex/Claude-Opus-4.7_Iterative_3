### Iteration 1


Prototype complete. All 13 tests pass on the first build attempt.

What was built
Solution layout (.NET 10, no external deps):

SubtitleQc.slnx
Directory.Packages.props        # central package versions (xUnit, etc.)
Directory.Build.props
SubtitleQc.Core/
  SubtitleQc.Core.csproj
  Models/
    Cue.cs                      # canonical internal model
  Qc/
    QcStatus.cs, QcResult.cs, QcReport.cs, RuleEngine.cs
    Abstractions/IQcRule.cs
    Rules/
      PerCueRuleBase.cs         # template for per-cue rules
      MaxLinesRule.cs, MaxCplRule.cs, MaxCpsRule.cs,
      MinDurationRule.cs, EmptyCueCheckRule.cs,
      OverlapCheckRule.cs       # cross-cue, implements IQcRule directly
  Parsers/
    Abstractions/ISubtitleParser.cs
    Internal/SubtitleBlockReader.cs, TimeCodeParser.cs, TimingLineParser.cs
    Srt/SrtParser.cs
    WebVtt/WebVttParser.cs
SubtitleQc.Tests/               # untouched .cs; added one ProjectReference only
test_metrics.jsonl              # attempt #1 logged: 13/13 (100%)
Architectural notes
OCP / DIP. RuleEngine depends only on IQcRule; new rules slot in without engine changes. ISubtitleParser plays the same role for new formats — the engine never sees a wire format.
SRP. Parsing, modeling, and validation live in disjoint namespaces. Even the parsers split shared infrastructure (block splitting, timecode parsing, timing-line parsing) into single-purpose Internal/ helpers so SRT and WebVTT can't drift on edge cases.
IQcRule over the cue collection (not single cue). This was a deliberate call: the iteration spec includes OverlapCheck, which is intrinsically cross-cue. Forcing all rules through the same collection-shaped contract keeps the engine uniform; per-cue rules inherit PerCueRuleBase to avoid boilerplate.
Boundary semantics. Each "max"/"min" rule uses strict inequality (> / <) to match the test's _ExactlyAt..._ShouldPass and _ExactlyAtMinimum_ShouldPass cases — boundary equality always passes.
Overlap "blame" rule. A cue is failed when an earlier-starting cue is still on screen at this cue's start (earlier.End > later.Start). Adjacent cues (end == next start) do not overlap.
Test telemetry
test_metrics.jsonl:


test_metrics.jsonl
Lines 1-1
{"project_iteration": 1, "attempt": 1, "timestamp": "2026-05-03T12:01:24.335Z", "build_successful": true, "total_tests": 13, "passed": 13, "failed": 0, "pass_rate_percentage": 100.0}
One documented assumption
The test csproj uses version-less <PackageReference> entries, so I added a root Directory.Packages.props enabling central package management with concrete versions for the existing xUnit/NUnit-SDK dependencies. I also added a <ProjectReference> to the Core project from the test csproj — this is the minimum necessary to let the existing using SubtitleQc.Core.* statements resolve. No test scenario, assertion, or .cs line was modified.