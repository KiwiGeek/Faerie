using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

/// <summary>Shaft-room basket — lower/raise between Shaft Room and Drafty Room (Infocom Zork I).</summary>
internal sealed partial class ZorkWorld
{
    private Verb _raise = null!;
    private Verb _lower = null!;

    private void DefineBasket()
    {
        _raise = _b.DefineVerb("raise", ["raise", "lift"],
            VerbForms.Transitive | VerbForms.Intransitive, RaiseHandler);
        _lower = _b.DefineVerb("lower", ["lower"],
            VerbForms.Transitive | VerbForms.Intransitive, LowerHandler);

        Basket.Describe(BasketDescription);

        _b.On(Basket).Before(_raise, RaiseBasketReaction);
        _b.On(Basket).Before(_lower, LowerBasketReaction);
    }

    private string BasketDescription(GameContext ctx)
    {
        if (ctx.Get(_basketLowered))
            return "At the end of the chain is a basket.";
        return "From the chain is suspended a basket.";
    }

    private VerbResult RaiseHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is not null && ctx.DirectObject != Basket)
        {
            ctx.Say("You can only raise the basket.");
            return VerbResult.Done;
        }

        return RaiseBasketReaction(ctx);
    }

    private VerbResult LowerHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is not null && ctx.DirectObject != Basket)
        {
            ctx.Say("You can only lower the basket.");
            return VerbResult.Done;
        }

        return LowerBasketReaction(ctx);
    }

    private VerbResult RaiseBasketReaction(VerbContext ctx)
    {
        if (!ctx.Get(_basketLowered))
        {
            ctx.Say("The basket is already at the top of the shaft.");
            return VerbResult.Done;
        }

        if (!ctx.InRoom(ShaftRoom) && !ctx.InRoom(DraftyRoom))
        {
            ctx.Say("You can't reach the basket from here.");
            return VerbResult.Done;
        }

        ctx.Set(_basketLowered, false);
        ctx.State.Move(Basket, Placement.InRoom(ShaftRoom));
        ctx.Say("The basket is raised to the top of the shaft.");
        return VerbResult.Done;
    }

    private VerbResult LowerBasketReaction(VerbContext ctx)
    {
        if (ctx.Get(_basketLowered))
        {
            ctx.Say("The basket is already at the bottom of the shaft.");
            return VerbResult.Done;
        }

        if (!ctx.InRoom(ShaftRoom))
        {
            ctx.Say("You can't reach the basket from here.");
            return VerbResult.Done;
        }

        ctx.Set(_basketLowered, true);
        ctx.State.Move(Basket, Placement.InRoom(DraftyRoom));
        ctx.Say("The basket is lowered to the bottom of the shaft.");
        return VerbResult.Done;
    }
}
