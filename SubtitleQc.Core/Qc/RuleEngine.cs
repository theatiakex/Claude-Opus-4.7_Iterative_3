using System;
using System.Collections.Generic;
using System.Linq;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc;

/// <summary>
/// Orchestrates execution of a configured set of <see cref="IQcRule"/> instances against
/// a cue collection. The engine has no knowledge of any specific rule (DIP) and is
/// therefore closed for modification when new rules are introduced (OCP).
/// </summary>
public sealed class RuleEngine
{
    private readonly IReadOnlyList<IQcRule> _rules;

    public RuleEngine(IEnumerable<IQcRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules.ToArray();
    }

    public QcReport Evaluate(IEnumerable<Cue> cues)
    {
        ArgumentNullException.ThrowIfNull(cues);
        IReadOnlyList<Cue> snapshot = cues.ToArray();
        List<QcResult> aggregated = new();
        foreach (IQcRule rule in _rules)
        {
            aggregated.AddRange(rule.Evaluate(snapshot));
        }
        return new QcReport(aggregated);
    }
}
