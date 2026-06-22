using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;
using static Faerie.Runtime.Scoring;

namespace Faerie.Samples.Zork;

internal sealed partial class ZorkWorld
{
    private StateKey<int> _trophyCaseScore = null!;
    private StateKey<int> _trophyTouchedMask = null!;
    private StateKey<int> _touchScoreMask = null!;

    private TrophyEntry[] _treasureScores = null!;

    private ReadOnlySpan<TrophyEntry> TreasureScores => _treasureScores;

    private void DefineScoringState()
    {
        _trophyCaseScore = _b.State("trophy-case-score", 0);
        _trophyTouchedMask = _b.State("trophy-touched-mask", 0);
        _touchScoreMask = _b.State("touch-score-mask", 0);

        // Egg and canary share case bit 1; each awards its own touch score (Infocom SCORE-TOUCH).
        _treasureScores =
        [
            new(Painting, 0, CasePoints: 6, TouchPoints: 4),
            new(JeweledEgg, 1, CasePoints: 5, TouchPoints: 5),
            new(GoldenCanary, 1, CasePoints: 4, TouchPoints: 6, TouchBit: 18),
            new(BrassBauble, 2, CasePoints: 1, TouchPoints: 1),
            new(BagOfCoins, 3, CasePoints: 5, TouchPoints: 10),
            new(PlatinumBar, 4, CasePoints: 5, TouchPoints: 10),
            new(TrunkOfJewels, 5, CasePoints: 5, TouchPoints: 15),
            new(CrystalTrident, 6, CasePoints: 11, TouchPoints: 4),
            new(IvoryTorch, 7, CasePoints: 6, TouchPoints: 14),
            new(GoldCoffin, 8, CasePoints: 15, TouchPoints: 10),
            new(Sceptre, 9, CasePoints: 6, TouchPoints: 4),
            new(CrystalSkull, 10, CasePoints: 10, TouchPoints: 10),
            new(LargeEmerald, 11, CasePoints: 10, TouchPoints: 5),
            new(Scarab, 12, CasePoints: 5, TouchPoints: 5),
            new(PotOfGold, 13, CasePoints: 10, TouchPoints: 10),
            new(JadeFigurine, 14, CasePoints: 5, TouchPoints: 5),
            new(SapphireBracelet, 15, CasePoints: 5, TouchPoints: 5),
            new(HugeDiamond, 16, CasePoints: 10, TouchPoints: 10),
            new(SilverChalice, 17, CasePoints: 5, TouchPoints: 10),
        ];
    }

    private void DefineTrophyScoring()
    {
        _b.Reactions.BeforeAny(ctx =>
        {
            if (ctx.Verb != _b.Verbs.Take || ctx.DirectObject is not { } taken)
                return VerbResult.Pass;

            TrophyEntry? entry = FindTreasureEntry(taken);
            if (entry is null || ctx.State.ContentsOf(TrophyCase).Contains(taken))
                return VerbResult.Pass;

            AwardTreasureTouch(ctx, _touchScoreMask, entry.Value);
            return VerbResult.Pass;
        });

        _b.Reactions.AfterAny(ctx =>
        {
            if (ctx.Verb is not null &&
                (ctx.Verb == _b.Verbs.Put && ctx.IndirectObject == TrophyCase ||
                 ctx.Verb == _b.Verbs.Take))
                SyncTrophyScore(ctx);
        });
    }

    private void DefineExplorationScore()
    {
        Kitchen.OnFirstEnter = ctx => AwardPlaceScore(ctx, 0, 10);
        Cellar.OnFirstEnter = ctx =>
        {
            if (AwardPlaceScore(ctx, 1, 25))
                ctx.Say("You have entered the cellar. (+25)");
        };
        TreasureRoom.OnFirstEnter = ctx => AwardPlaceScore(ctx, 2, 25);
        EwPassage.OnFirstEnter = ctx => AwardPlaceScore(ctx, 3, 5);
        DraftyRoom.OnFirstEnter = ctx => AwardPlaceScore(ctx, 4, 13);
    }

    private void SyncTrophyScore(GameContext ctx)
    {
        int touchedMask = SyncTrophyCase(
            ctx,
            TrophyCase,
            _trophyCaseScore,
            _trophyTouchedMask,
            TreasureScores);

        if (CountBits(touchedMask, TreasureCount) >= TreasureCount && !ctx.Get(_allTreasuresInCase))
        {
            ctx.Set(_allTreasuresInCase, true);
            ctx.Set(_wonFlag, true);
            ctx.Say("{fg:gold}Your collection of treasures is complete! A map materializes in the " +
                    "trophy case, hinting at a final secret.{/}");
        }
    }

    private bool AwardPlaceScore(GameContext ctx, int bit, int points) =>
        AwardOnce(ctx, _placeScoreMask, bit, points);

    private TrophyEntry? FindTreasureEntry(Thing treasure)
    {
        foreach (TrophyEntry entry in TreasureScores)
        {
            if (entry.Treasure == treasure)
                return entry;
        }

        return null;
    }
}
