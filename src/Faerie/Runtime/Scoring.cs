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
    /// <param name="Bit">Trophy-case slot; treasures sharing a bit contribute only the highest <see cref="CasePoints"/> present.</param>
    /// <param name="TouchPoints">One-time points for first take.</param>
    /// <param name="TouchBit">Touch-score bit; defaults to <paramref name="Bit"/>. Use a distinct bit when touch points are independent (e.g. Zork egg and canary).</param>
    public readonly record struct TrophyEntry(Thing Treasure, int Bit, int CasePoints, int TouchPoints = 0, int TouchBit = -1)
    {
        public int EffectiveTouchBit => TouchBit >= 0 ? TouchBit : Bit;
    }

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

        // Treasures sharing a case bit (e.g. Zork egg and canary) score once — the highest value present.
        for (int bit = 0; bit < 32; bit++)
        {
            int bestCase = 0;
            bool present = false;
            foreach (TrophyEntry entry in entries)
            {
                if (entry.Bit != bit || !contents.Contains(entry.Treasure)) continue;
                present = true;
                bestCase = Math.Max(bestCase, entry.CasePoints);
            }

            if (!present) continue;
            newSum += bestCase;
            touchedMask |= 1 << bit;
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
        return AwardOnce(ctx, touchMaskKey, entry.EffectiveTouchBit, entry.TouchPoints);
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
