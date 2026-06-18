using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

/// <summary>Hades exorcism — Infocom bell / candles / black book ritual (historicalsource/zork1).</summary>
internal sealed partial class ZorkWorld
{
    private const int HadesCeremonyTimeoutTurns = 10;

    private StateKey<int> _hadesCeremonyStage = null!;
    private StateKey<int> _hadesCeremonyTimer = null!;
    private StateKey<bool> _hadesBellHot = null!;

    private Verb _pour = null!;

    private void DefineHades()
    {
        _hadesCeremonyStage = _b.State("hades-ceremony-stage", 0);
        _hadesCeremonyTimer = _b.State("hades-ceremony-timer", 0);
        _hadesBellHot = _b.State("hades-bell-hot", false);

        _pour = _b.DefineVerb("pour", ["pour"], VerbForms.Ditransitive | VerbForms.Transitive, PourHandler);

        BrassBell.Describe(ctx => ctx.Get(_hadesBellHot)
            ? "On the ground is a red hot brass bell."
            : "A brass bell.");

        EntranceToHades.OnFirstEnter = ctx =>
        {
            if (!ctx.Get(_hadesOpen))
                ctx.Say("The way through the gate is barred by evil spirits, who jeer at your attempts to pass.");
        };

        _b.On(BrassBell).Before(_ring, RingBellAtHades);
        _b.On(BrassBell).Before(_b.Verbs.Take!, HotBellTakeHandler);
        _b.On(BrassBell).Before(_pour, PourOnBellHandler);
        _b.On(PairOfCandles).Before(_b.Verbs.SwitchOn!, RelightCeremonyCandles);
        _b.On(BlackBook).Before(_b.Verbs.Read!, ReadBlackBook);
        _b.EveryTurn(HadesCeremonyTimeoutTick, when: ctx =>
            ctx.Get(_hadesCeremonyStage) > 0 && !ctx.Get(_hadesOpen));
    }

    private VerbResult RingBellAtHades(VerbContext ctx)
    {
        if (!ctx.InRoom(EntranceToHades) || ctx.Get(_hadesOpen)) return VerbResult.Pass;
        if (!ctx.Carrying(BrassBell))
        {
            ctx.Say("The bell makes a hollow sound.");
            return VerbResult.Done;
        }

        if (!ctx.Carrying(PairOfCandles))
        {
            ctx.Say("The bell makes a hollow sound.");
            return VerbResult.Done;
        }

        ctx.Remove(BrassBell);
        ctx.PlaceHere(BrassBell);
        ctx.Set(_hadesBellHot, true);

        ctx.Remove(PairOfCandles);
        PairOfCandles.Set(Attr.Lit, false);
        ctx.PlaceHere(PairOfCandles);

        ctx.Set(_hadesCeremonyStage, 1);
        TouchCeremonyTimer(ctx);

        ctx.Say(
            "The bell suddenly becomes red hot and falls to the ground. " +
            "The wraiths, as if paralyzed, stop their jeering and slowly turn to face you. " +
            "On their ashen faces, the expression of a long-forgotten terror takes shape.");
        ctx.Say("In your confusion, the candles drop to the ground (and they are out).");
        return VerbResult.Done;
    }

    private VerbResult HotBellTakeHandler(VerbContext ctx)
    {
        if (!ctx.Get(_hadesBellHot)) return VerbResult.Pass;
        ctx.Say("The bell is still too hot to touch.");
        return VerbResult.Done;
    }

    private VerbResult PourOnBellHandler(VerbContext ctx)
    {
        if (ctx.DirectObject != BrassBell || !ctx.Get(_hadesBellHot)) return VerbResult.Pass;
        if (ctx.IndirectObject != Bottle && !ctx.Carrying(Bottle))
        {
            ctx.Say("Pour what on the bell?");
            return VerbResult.Done;
        }

        ctx.Say("The water spills over the red hot brass bell and to the floor where it evaporates.");
        ctx.Set(_hadesBellHot, false);
        TouchCeremonyTimer(ctx);
        return VerbResult.Done;
    }

    private VerbResult RelightCeremonyCandles(VerbContext ctx)
    {
        if (!ctx.InRoom(EntranceToHades) || ctx.Get(_hadesCeremonyStage) != 1) return VerbResult.Pass;
        if (!ctx.Carrying(PairOfCandles))
        {
            ctx.Say("You need to be holding the candles.");
            return VerbResult.Done;
        }

        if (!HasMatchSource(ctx))
        {
            ctx.Say("You need a match to light the candles.");
            return VerbResult.Done;
        }

        if (PairOfCandles.Has(Attr.Lit))
        {
            ctx.Say("The pair of candles is already lit.");
            return VerbResult.Done;
        }

        PairOfCandles.Set(Attr.Lit, true);
        ctx.Set(_hadesCeremonyStage, 2);
        TouchCeremonyTimer(ctx);

        ctx.Say("The candles are lit.");
        ctx.Say(
            "The flames flicker wildly and appear to dance. The earth beneath your feet trembles, " +
            "and your legs nearly buckle beneath you. The spirits cower at your unearthly power.");
        return VerbResult.Done;
    }

    private VerbResult ReadBlackBook(VerbContext ctx)
    {
        if (!ctx.InRoom(EntranceToHades) || ctx.Get(_hadesCeremonyStage) != 2)
        {
            ctx.Say("The book is written in an unknown tongue. You cannot read it.");
            return VerbResult.Done;
        }

        if (!ctx.Carrying(BlackBook))
        {
            ctx.Say("You need to be holding the book.");
            return VerbResult.Done;
        }

        if (!ctx.Carrying(PairOfCandles) || !PairOfCandles.Has(Attr.Lit))
        {
            ctx.Say("The book is written in an unknown tongue. You cannot read it.");
            return VerbResult.Done;
        }

        ctx.Set(_hadesOpen, true);
        ctx.Set(_hadesCeremonyStage, 0);
        ctx.Set(_hadesCeremonyTimer, 0);

        ctx.Say(
            "Each word of the prayer reverberates through the hall in a deafening confusion. " +
            "As the last word fades, a voice, loud and commanding, speaks: \"Begone, fiends!\" " +
            "A heart-stopping scream fills the cavern, and the spirits, sensing a greater power, flee through the walls.");
        return VerbResult.Done;
    }

    private void HadesCeremonyTimeoutTick(GameContext ctx)
    {
        if (!ctx.InRoom(EntranceToHades)) return;

        int turns = ctx.Get(_hadesCeremonyTimer) + 1;
        ctx.Set(_hadesCeremonyTimer, turns);
        if (turns < HadesCeremonyTimeoutTurns) return;

        BreakCeremony(ctx);
        ctx.Say(
            "The tension of this ceremony is broken, and the wraiths, amused but shaken at your clumsy attempt, " +
            "resume their hideous jeering.");
    }

    private void BreakCeremony(GameContext ctx)
    {
        ctx.Set(_hadesCeremonyStage, 0);
        ctx.Set(_hadesCeremonyTimer, 0);
    }

    private void TouchCeremonyTimer(GameContext ctx) => ctx.Set(_hadesCeremonyTimer, 0);

    private bool HasMatchSource(VerbContext ctx) =>
        ctx.IndirectObject == Matchbook ||
        ctx.Carrying(Matchbook) ||
        (ctx.IndirectObject == Lantern && Lantern.Has(Attr.Lit));

    private VerbResult PourHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == Bottle) return VerbResult.Pass;
        ctx.Say("Pour what?");
        return VerbResult.Done;
    }
}
