using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

/// <summary>Infocom combat refinements — fight strength, weaknesses, throw, thief flee.</summary>
internal sealed partial class ZorkWorld
{
    private Verb _throw = null!;

    private void WireCombatFidelity()
    {
        _throw = _b.DefineVerb("throw", ["throw", "hurl", "fling"],
            VerbForms.Transitive | VerbForms.Ditransitive, ThrowHandler);

        _b.On(Troll).Before(_throw, ThrowAtTroll);
        _b.On(Thief).Before(_throw, ThrowAtVillain);
    }

    /// <summary>Infocom FIGHT-STRENGTH: 2 at start, +1 per 70 points, max 7.</summary>
    private int PlayerFightStrength(GameContext ctx)
    {
        int baseStr = Math.Clamp(2 + ctx.State.Score / 70, 2, 7);
        int wounds = PlayerMaxHp - ctx.Get(_playerHp);
        return Math.Max(1, baseStr - wounds);
    }

    private int VillainCombatStrength(GameContext ctx, Thing villain, Thing? weapon, StateKey<int> hpKey)
    {
        int str = ctx.Get(hpKey);
        if (villain == Troll && weapon == Sword)
            str = Math.Max(1, str - 1);
        if (villain == Thief && weapon == NastyKnife)
            str = Math.Max(1, str - 1);
        if (villain == Thief && ctx.Get(_thiefEngrossed))
        {
            str = Math.Min(str, 2);
            ctx.Set(_thiefEngrossed, false);
        }

        return str;
    }

    private bool IsWeaknessWeapon(Thing villain, Thing? weapon) =>
        villain == Troll && weapon == Sword || villain == Thief && weapon == NastyKnife;

    private int CombatRollBonus(GameContext ctx, Thing villain, Thing? weapon, StateKey<int> hpKey)
    {
        int scoreBonus = (PlayerFightStrength(ctx) - 2) * 5;
        int weaknessBonus = IsWeaknessWeapon(villain, weapon) ? 8 : 0;
        return scoreBonus + weaknessBonus;
    }

    private bool ThiefShouldFlee(GameContext ctx)
    {
        if (ctx.Get(_thiefDead) || ctx.Get(_thiefKO) > 0 || !ctx.Here(Thief) || Thief.Has(Attr.Concealed))
            return false;
        if (ctx.InRoom(TreasureRoom))
            return false;

        const int thiefFullStrength = 4;
        if (ctx.Get(_thiefHp) < thiefFullStrength)
            return false;

        int player = PlayerFightStrength(ctx);
        int thief = ctx.Get(_thiefHp);
        return player > thief && ctx.Random.Next(10) < 4;
    }

    private void ThiefFleesCombat(GameContext ctx)
    {
        bool robbed = RobPlayerTreasures(ctx, 100);
        ctx.Say(robbed
            ? "The thief, finding you a formidable opponent, snatches what he can and slips away."
            : "The thief decides discretion is the better part of valor and slips away.");
        HideThief(ctx);
        MoveThiefToNextRoom(ctx);
        ctx.Set(_thiefHere, false);
    }

    private VerbResult ThrowHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null)
        {
            ctx.Say("Throw what?");
            return VerbResult.Done;
        }

        if (ctx.IndirectObject is { } target && (target == Troll || target == Thief))
            return VerbResult.Pass;

        if (ctx.Here(Troll))
            return ThrowAtTroll(ctx);
        if (ctx.Here(Thief) && !ctx.Get(_thiefDead))
            return ThrowAtVillain(ctx);

        ctx.Say("You can't throw that.");
        return VerbResult.Done;
    }

    private VerbResult ThrowAtTroll(VerbContext ctx)
    {
        if (!ctx.Here(Troll) || ctx.Get(_trollDefeated)) return VerbResult.Pass;
        Thing? weapon = ctx.DirectObject;
        if (weapon is null || !IsWeapon(weapon))
        {
            ctx.Say("The troll is not interested.");
            return VerbResult.Done;
        }

        if (!ctx.Carrying(weapon))
        {
            ctx.Say("You don't have that.");
            return VerbResult.Done;
        }

        ctx.Remove(weapon);
        if (ctx.Random.Next(10) < 2)
        {
            ctx.Say($"The troll catches the {weapon.Name}, examines it carefully, and eats it! He burps and collapses, dead.");
            KillTroll(ctx);
            return VerbResult.Done;
        }

        ctx.PlaceHere(weapon);
        ctx.Say($"The troll catches the {weapon.Name} and examines it with interest.");
        return VerbResult.Done;
    }

    private VerbResult ThrowAtVillain(VerbContext ctx)
    {
        if (!ctx.Here(Thief) || ctx.Get(_thiefDead)) return VerbResult.Pass;
        Thing? weapon = ctx.DirectObject;
        if (weapon is null || !IsWeapon(weapon))
        {
            ctx.Say("The thief is not impressed.");
            return VerbResult.Done;
        }

        if (!ctx.Carrying(weapon))
        {
            ctx.Say("You don't have that.");
            return VerbResult.Done;
        }

        ctx.Remove(weapon);
        ctx.PlaceHere(weapon);
        ctx.Say($"You throw the {weapon.Name} at the thief. It clatters to the floor.");
        return PlayerStrikes(ctx, Thief, _thiefHp, _thiefKO, _thiefDead, "thief", KillThief);
    }
}
