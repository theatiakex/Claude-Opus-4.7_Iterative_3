using System;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails any cue whose start frame is fewer than the configured number of frames away
/// from any known shot change. Strict less-than is used so that a cue exactly at the
/// threshold passes - matching the editorial convention that a frame-equal-to-limit
/// distance is the smallest legal gap. Cues without frame metadata are conservatively
/// passed (the rule has nothing to assert against), which is documented behaviour
/// rather than a silent failure.
/// </summary>
public sealed class MinFramesFromShotChangeRule : PerCueRuleBase
{
    public const string RuleName = "MinFramesFromShotChange";

    private readonly IShotChangeProvider _shotChangeProvider;
    private readonly int _thresholdFrames;

    public MinFramesFromShotChangeRule(IShotChangeProvider shotChangeProvider, int thresholdFrames)
    {
        ArgumentNullException.ThrowIfNull(shotChangeProvider);
        if (thresholdFrames < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(thresholdFrames), "Frame threshold cannot be negative.");
        }
        _shotChangeProvider = shotChangeProvider;
        _thresholdFrames = thresholdFrames;
    }

    public override string Name => RuleName;

    protected override QcResult EvaluateCue(Cue cue)
    {
        if (cue.StartFrame is null)
        {
            return QcResult.Pass(cue.Id, RuleName);
        }
        return EvaluateAgainstCuts(cue, cue.StartFrame.Value);
    }

    private QcResult EvaluateAgainstCuts(Cue cue, int startFrame)
    {
        foreach (int cutFrame in _shotChangeProvider.GetShotChangeFrames())
        {
            int distance = Math.Abs(startFrame - cutFrame);
            if (distance < _thresholdFrames)
            {
                string message = $"Start frame {startFrame} is {distance} frames from cut at {cutFrame} (threshold {_thresholdFrames}).";
                return QcResult.Fail(cue.Id, RuleName, message);
            }
        }
        return QcResult.Pass(cue.Id, RuleName);
    }
}
