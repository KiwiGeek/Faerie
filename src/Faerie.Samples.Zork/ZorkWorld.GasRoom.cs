using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

internal sealed partial class ZorkWorld
{
    private void DefineGasRoom()
    {
        const string enterMessage =
            "Oh dear. It appears that the smell coming from this room was coal gas. " +
            "I would have thought twice about carrying flaming objects in here.";
        const string lightMessage =
            "How sad for an aspiring adventurer to light a flame in a room which reeks of gas. " +
            "Fortunately, there is justice in the world.";
        const string boom = "\n      ** BOOOOOOOOOOOM **";

        void Explode(GameContext ctx, string preamble) => ctx.Die(preamble + boom);

        GasRoom.HazardOnEnter(Hazards.HasCarriedOpenFlame, ctx => Explode(ctx, enterMessage));
        _b.HazardEveryTurn(GasRoom, Hazards.HasCarriedOpenFlame, ctx => Explode(ctx, enterMessage));

        _b.Reactions.BeforeAny(ctx =>
        {
            if (ctx.State.IsOver || !ctx.InRoom(GasRoom)) return VerbResult.Pass;
            if (ctx.Verb?.Id != StandardVerbIds.PushButton) return VerbResult.Pass;
            if (ctx.DirectObject is not Thing thing || !thing.Has(Attr.Flame)) return VerbResult.Pass;
            if (thing.Has(Attr.Lit)) return VerbResult.Pass;

            Explode(ctx, lightMessage);
            return VerbResult.Done;
        });
    }
}
