using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

/// <summary>Altar pray — Infocom instant teleport to surface forest (historicalsource/zork1).</summary>
internal sealed partial class ZorkWorld
{
    private bool TryAltarPray(VerbContext ctx)
    {
        if (!ctx.InRoom(Altar)) return false;
        ctx.MovePlayerTo(R(ZorkIds.Forest1));
        return true;
    }
}
