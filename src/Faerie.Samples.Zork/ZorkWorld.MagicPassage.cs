using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

/// <summary>Magic passage / gnome — Infocom gallery gnome and chimney flag (historicalsource/zork1).</summary>
internal sealed partial class ZorkWorld
{
    private StateKey<bool> _gnomeMet = null!;

    private void DefineMagicAndChimney()
    {
        _gnomeMet = _b.State("gnome-met", false);

        Gallery.OnFirstEnter = ctx =>
        {
            if (ctx.Get(_gnomeMet)) return;
            ctx.Set(_gnomeMet, true);
            ctx.Say("A small gnome with a long, pointed beard appears out of the shadows. " +
                    "\"I don't believe we've been introduced,\" he says. \"I am the Keeper of the Zork Treasury. " +
                    "Please give me your valuables!\"");
        };

        _b.Reactions.BeforeAny(GnomeGiveHandler);
    }

    private VerbResult GnomeGiveHandler(VerbContext ctx)
    {
        if (ctx.Verb != _b.Verbs.Give || !ctx.InRoom(Gallery) || ctx.Get(_magicFlag))
            return VerbResult.Pass;

        Thing? gift = ctx.DirectObject;
        if (gift is null || !ctx.Carrying(gift)) return VerbResult.Pass;

        if (!IsTreasure(gift))
        {
            ctx.Say("The gnome is not interested in your gift.");
            return VerbResult.Done;
        }

        ctx.Remove(gift);
        ctx.Set(_magicFlag, true);
        ctx.Say("The gnome takes your treasure, mumbles something about \"fair compensation,\" " +
                "and disappears into the gloom.");
        return VerbResult.Done;
    }
}
