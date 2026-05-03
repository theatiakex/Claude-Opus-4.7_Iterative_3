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


### Iteration 2

Iteration 2 complete in one attempt, 13/13 tests still green.

What changed (additions only — zero modifications to existing code)
SubtitleQc.Core/Parsers/
  Internal/
    TtmlTimeExpressionParser.cs      # new
  Ttml/
    TtmlParser.cs                    # new — implements ISubtitleParser
Files modified: none. The QC engine, rules, internal model, SRT/WebVTT parsers, and the test project file are untouched. This is the OCP guarantee from iteration 1 paying off — adding a format means adding a class, not changing one.

Design notes for the new parser
Local-name lookups everywhere. TTML is officially in http://www.w3.org/ns/ttml but real-world docs vary (default namespace, prefixes, or none). EnumerateParagraphs, FindAttribute, and the <br/> detector all use Name.LocalName so the parser accepts every plausible namespace declaration without baking one in.
Two grammars, one attribute. TTML's begin/end/dur accept both clock-time (00:00:01.500) and offset-time (5s, 1500ms, 2.5h). TtmlTimeExpressionParser dispatches by the presence of : and reuses the existing TimeCodeParser for the clock-time branch — no duplication with SRT/WebVTT.
dur support, derived end. When <p> has begin + dur but no end, end is computed as begin + dur. This is part of standard TTML and trivial to support correctly.
Frame/tick units intentionally unsupported. The f (frames) and t (ticks) metrics require frame-rate metadata declared on the TTML root. Pulling that in is out of scope for iteration 2 and would couple the parser to TTML head metadata. A FormatException is raised if encountered, with a documented comment explaining why.
Line splitting walks the DOM. <br/> (in any namespace) flushes the current line; nested <span> content folds into the surrounding line. Implemented with a small recursive walker so each method stays under the 20-line cap.
No engine touches. TtmlParser only depends on Cue and ISubtitleParser. The QC RuleEngine will validate the resulting IReadOnlyList<Cue> by simply calling engine.Evaluate(parser.Parse(xml)) — exactly as required by the acceptance criterion.
Telemetry

test_metrics.jsonl
Lines 1-2
{"project_iteration": 1, "attempt": 1, "timestamp": "2026-05-03T12:01:24.335Z", "build_successful": true, "total_tests": 13, "passed": 13, "failed": 0, "pass_rate_percentage": 100.0}
{"project_iteration": 2, "attempt": 1, "timestamp": "2026-05-03T12:10:20.300Z", "build_successful": true, "total_tests": 13, "passed": 13, "failed": 0, "pass_rate_percentage": 100.0}
