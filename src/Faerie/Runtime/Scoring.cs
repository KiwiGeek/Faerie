using Faerie.Model;

namespace Faerie.Runtime;

/// <summary>Score adjustment helpers for games with bonuses, penalties, and trophy-case scoring.</summary>
public static class Scoring
{
    /// <summary>
    /// Adds <paramref name="delta"/> to the score, clamping to <c>[0, maxScore]</c> when
    /// <paramref name="maxScore"/> is positive.
    /// </summary>
    public static int AdjustScore(GameState state, int delta, int maxScore = 0)
    {
        int next = state.Score + delta;
        state.Score = maxScore > 0 ? Math.Clamp(next, 0, maxScore) : Math.Max(0, next);
        return state.Score;
    }

    /// <summary>Adjusts score using <see cref="GameEngine.MaxScore"/> when set.</summary>
    public static int Adjust(GameContext ctx, int delta, string? message = null)
    {
        int score = AdjustScore(ctx.State, delta, ctx.Engine.MaxScore);
        if (message is not null) ctx.Say(message);
        return score;
    }

    /// <summary>Awards <paramref name="points"/> once per <paramref name="bit"/> in <paramref name="maskKey"/>.</summary>
    public static bool AwardOnce(
        GameContext ctx,
        StateKey<int> maskKey,
        int bit,
        int points,
        string? message = null)
    {
        int mask = ctx.Get(maskKey);
        if ((mask & (1 << bit)) != 0) return false;

        ctx.Set(maskKey, mask | (1 << bit));
        Adjust(ctx, points, message);
        return true;
    }

    /// <summary>One treasure tracked for dynamic trophy-case scoring.</summary>
    public readonly record struct TrophyEntry(Thing Treasure, int Bit, int CasePoints, int TouchPoints = 0);

    /// <summary>
    /// Recomputes trophy-case points from current contents. Score changes when treasures are put in
    /// or taken out. Sets bits in <paramref name="touchedMaskKey"/> when a treasure is deposited.
    /// </summary>
    public static int SyncTrophyCase(
        GameContext ctx,
        Thing container,
        StateKey<int> caseScoreKey,
        StateKey<int> touchedMaskKey,
        ReadOnlySpan<TrophyEntry> entries,
        string? gainMessage = null)
    {
        HashSet<Thing> contents = ctx.State.ContentsOf(container).ToHashSet();
        int newSum = 0;
        int touchedMask = ctx.Get(touchedMaskKey);

        foreach (TrophyEntry entry in entries)
        {
            if (!contents.Contains(entry.Treasure)) continue;
            newSum += entry.CasePoints;
            touchedMask |= 1 << entry.Bit;
        }

        int lastSum = ctx.Get(caseScoreKey);
        int delta = newSum - lastSum;
        ctx.Set(caseScoreKey, newSum);
        ctx.Set(touchedMaskKey, touchedMask);

        if (delta > 0)
        {
            string message = gainMessage ?? $"You have gained {delta} points.";
            Adjust(ctx, delta, message);
        }
        else if (delta < 0)
            Adjust(ctx, delta);

        return touchedMask;
    }

    /// <summary>Awards <paramref name="entry"/>.<see cref="TrophyEntry.TouchPoints"/> the first time a treasure is taken.</summary>
    public static bool AwardTreasureTouch(
        GameContext ctx,
        StateKey<int> touchMaskKey,
        TrophyEntry entry)
    {
        if (entry.TouchPoints <= 0) return false;
        return AwardOnce(ctx, touchMaskKey, entry.Bit, entry.TouchPoints);
    }

    /// <summary>Counts how many bits are set in a mask (up to <paramref name="maxBits"/>).</summary>
    public static int CountBits(int mask, int maxBits)
    {
        int count = 0;
        for (int i = 0; i < maxBits; i++)
            if ((mask & (1 << i)) != 0) count++;
        return count;
    }
}
