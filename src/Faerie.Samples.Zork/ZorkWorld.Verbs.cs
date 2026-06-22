using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;
using static Faerie.Runtime.Fluid;

namespace Faerie.Samples.Zork;

/// <summary>Additional Infocom verbs — smell, listen, count, fill, etc. (#38).</summary>
internal sealed partial class ZorkWorld
{
    private Verb _smell = null!;
    private Verb _listen = null!;
    private Verb _count = null!;
    private Verb _shake = null!;
    private Verb _fill = null!;
    private Verb _answer = null!;
    private Verb _plug = null!;

    private void DefineZorkVerbs()
    {
        _smell = _b.DefineVerb("smell", ["smell", "sniff"],
            VerbForms.Transitive | VerbForms.Intransitive, SmellHandler);
        _listen = _b.DefineVerb("listen", ["listen"],
            VerbForms.Transitive | VerbForms.Intransitive, ListenHandler);
        _count = _b.DefineVerb("count", ["count"],
            VerbForms.Transitive, CountHandler);
        _shake = _b.DefineVerb("shake", ["shake"],
            VerbForms.Transitive, ShakeHandler);
        _fill = _b.DefineVerb("fill", ["fill"],
            VerbForms.Transitive | VerbForms.Ditransitive, FillHandler);
        _answer = _b.DefineVerb("answer", ["answer", "reply"],
            VerbForms.Transitive | VerbForms.Intransitive, AnswerHandler);
        _plug = _b.DefineVerb("plug", ["plug"],
            VerbForms.Transitive | VerbForms.Ditransitive, PlugHandler);
    }

    private VerbResult SmellHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null)
        {
            if (ctx.InRoom(GasRoom))
            {
                ctx.Say("It smells like coal gas in here.");
                return VerbResult.Done;
            }

            if (ctx.InRoom(R(ZorkIds.SmellyRoom)))
            {
                ctx.Say("The room smells awful.");
                return VerbResult.Done;
            }

            ctx.Say("Smell what?");
            return VerbResult.Done;
        }

        if (ctx.DirectObject == Garlic)
        {
            ctx.Say("It smells strongly of garlic.");
            return VerbResult.Done;
        }

        if (ctx.DirectObject == Lunch)
        {
            ctx.Say("It smells like a hot pepper sandwich.");
            return VerbResult.Done;
        }

        if (ctx.DirectObject == Sack)
        {
            ctx.Say("It smells of hot peppers.");
            return VerbResult.Done;
        }

        ctx.Say($"It smells just like a {ctx.DirectObject.Name}.");
        return VerbResult.Done;
    }

    private VerbResult ListenHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is not null)
        {
            ctx.Say("You hear nothing special.");
            return VerbResult.Done;
        }

        if (ctx.InRoom(LoudRoom) && !LoudRoomQuiet(ctx))
        {
            ctx.Say("It's not a pretty sound.");
            return VerbResult.Done;
        }

        if (ctx.InRoom(Dam))
        {
            ctx.Say("The dam rumbles loudly.");
            return VerbResult.Done;
        }

        if (ctx.InRoom(R(ZorkIds.AragainFalls)))
        {
            ctx.Say("You can hear the water crashing over the falls.");
            return VerbResult.Done;
        }

        if (ctx.InRoom(GasRoom))
        {
            ctx.Say("The room is quiet.");
            return VerbResult.Done;
        }

        if (ctx.InRoom(DeepCanyon))
        {
            ctx.Say("You can hear the river below.");
            return VerbResult.Done;
        }

        ctx.Say("You hear nothing unusual.");
        return VerbResult.Done;
    }

    private VerbResult CountHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null)
        {
            ctx.Say("Count what?");
            return VerbResult.Done;
        }

        if (ctx.DirectObject == BagOfCoins || ctx.DirectObject.Nouns.Contains("coins"))
        {
            ctx.Say("There are a large number of coins in the bag.");
            return VerbResult.Done;
        }

        ctx.Say("You have lost count.");
        return VerbResult.Done;
    }

    private VerbResult ShakeHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null)
        {
            ctx.Say("Shake what?");
            return VerbResult.Done;
        }

        if (ctx.DirectObject == Bottle)
        {
            if (ContainerHolds(ctx.State, Bottle, Water))
                ctx.Say("The water sloshes around noisily.");
            else
                ctx.Say("The bottle rattles for a moment.");
            return VerbResult.Done;
        }

        ctx.Say("You shake it, but nothing happens.");
        return VerbResult.Done;
    }

    private VerbResult FillHandler(VerbContext ctx)
    {
        Thing? target = ctx.DirectObject;
        if (target is null)
        {
            ctx.Say("Fill what?");
            return VerbResult.Done;
        }

        if (target == Water)
            target = Bottle;

        if (target != Bottle)
        {
            ctx.Say("You can't fill that.");
            return VerbResult.Done;
        }

        if (!ctx.Carrying(Bottle))
        {
            ctx.Say("You don't have it.");
            return VerbResult.Done;
        }

        if (!IsWaterSourceRoom(ctx.CurrentRoom))
        {
            ctx.Say("You can't fill it here.");
            return VerbResult.Done;
        }

        if (ContainerHolds(ctx.State, Bottle, Water))
        {
            ctx.Say("The bottle is already full.");
            return VerbResult.Done;
        }

        if (!Bottle.Has(Attr.Open))
            Bottle.Set(Attr.Open, true);

        ctx.State.Move(Water, Placement.Inside(Bottle));
        ctx.Say("The bottle is now full of water.");
        return VerbResult.Done;
    }

    private VerbResult AnswerHandler(VerbContext ctx)
    {
        if (ctx.InRoom(LoudRoom) && !LoudRoomQuiet(ctx))
            return EchoHandler(ctx);

        ctx.Say("Nothing happens.");
        return VerbResult.Done;
    }

    private VerbResult PlugHandler(VerbContext ctx)
    {
        if ((ctx.DirectObject == Putty && ctx.IndirectObject == Leak) ||
            (ctx.DirectObject == Leak && ctx.IndirectObject == Putty))
        {
            if (!ctx.InRoom(MaintenanceRoom) || !ctx.Carrying(Putty))
            {
                ctx.Say("You can't plug the leak from here.");
                return VerbResult.Done;
            }

            FixMaintenanceLeak(ctx);
            return VerbResult.Done;
        }

        ctx.Say("Plug what with what?");
        return VerbResult.Done;
    }

    private static bool IsWaterSourceRoom(Room room) =>
        room.Name is "Stream" or "Reservoir" or "Frigid River" or "Dam Base" or "Shore"
            || room.Name.StartsWith("Frigid River", StringComparison.Ordinal);
}
