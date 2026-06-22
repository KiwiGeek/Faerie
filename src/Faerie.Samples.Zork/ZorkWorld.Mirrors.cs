using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

internal sealed partial class ZorkWorld
{
    internal Room MirrorRoom1 => R(ZorkIds.MirrorRoom1);
    internal Room MirrorRoom2 => R(ZorkIds.MirrorRoom2);

    private StateKey<bool> _mirrorBroken = null!;
    private StateKey<bool> _lucky = null!;
    private MirrorPair _mirrors = null!;
    private Verb _rub = null!;

    private void DefineMirrors()
    {
        _mirrorBroken = _b.State("mirror-broken", false);
        _lucky = _b.State("lucky", true);

        _mirrors = _b.MirrorRooms(MirrorRoom1, MirrorRoom2, _mirrorBroken);
        Mirror1.MirrorIn(_mirrors, MirrorRoom1)
            .Describe("An enormous mirror fills the south wall.");
        Mirror2.MirrorIn(_mirrors, MirrorRoom2)
            .Describe("An enormous mirror fills the north wall.");

        MirrorRoom1.Describe(MirrorRoom1Look);
        MirrorRoom2.Describe(MirrorRoom2Look);

        WireMirror(Mirror1);
        WireMirror(Mirror2);
    }

    private void WireMirror(Thing mirror)
    {
        _b.On(mirror).Before(_rub, MirrorRub);
        _b.On(mirror).Before(_b.Verbs.Examine!, MirrorExamine);
        _b.On(mirror).Before(_b.Verbs.Break!, MirrorBreak);
        _b.On(mirror).Before(_attack, MirrorBreak);
        _b.On(mirror).Before(_move, MirrorBreak);

        mirror.OnTake = ctx =>
        {
            ctx.Say("The mirror is many times your size. Give up.");
            return true;
        };
    }

    private VerbResult MirrorRub(VerbContext ctx)
    {
        MirrorPair pair = ctx.DirectObject!.MirrorLink!;
        if (pair.IsBroken(ctx)) return VerbResult.Pass;

        if (ctx.IndirectObject is Thing with)
        {
            ctx.Say($"You feel a faint tingling transmitted through the {with.Name}.");
            return VerbResult.Done;
        }

        pair.SwapContents(ctx);
        ctx.MovePlayerTo(pair.Partner(ctx.CurrentRoom));
        ctx.Say("There is a rumble from deep within the earth and the room shakes.");
        return VerbResult.Done;
    }

    private VerbResult MirrorExamine(VerbContext ctx)
    {
        MirrorPair pair = ctx.DirectObject!.MirrorLink!;
        if (pair.IsBroken(ctx))
            ctx.Say("The mirror is broken into many pieces.");
        else
            ctx.Say("There is an ugly person staring back at you.");
        return VerbResult.Done;
    }

    private VerbResult MirrorBreak(VerbContext ctx)
    {
        MirrorPair pair = ctx.DirectObject!.MirrorLink!;
        if (pair.IsBroken(ctx))
        {
            ctx.Say("Haven't you done enough damage already?");
            return VerbResult.Done;
        }

        ctx.Set(_mirrorBroken, true);
        ctx.Set(_lucky, false);
        Mirror1.Set(Attr.Broken);
        Mirror2.Set(Attr.Broken);
        ctx.Say(
            "You have broken the mirror. I hope you have a seven years' supply of good luck handy.");
        return VerbResult.Done;
    }

    private string MirrorRoom1Look(GameContext ctx)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(
            "You are in a large square room with tall ceilings. On the south wall is an enormous mirror which fills the entire wall. There are exits on the other three sides of the room.");
        if (ctx.Get(_mirrorBroken))
            sb.Append("\nUnfortunately, the mirror has been destroyed by your recklessness.");
        sb.Append('\n');
        return sb.ToString();
    }

    private string MirrorRoom2Look(GameContext ctx)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(
            "You are in a large square room with tall ceilings. On the north wall is an enormous mirror which fills the entire wall. There are exits on the other three sides of the room.");
        if (ctx.Get(_mirrorBroken))
            sb.Append("\nUnfortunately, the mirror has been destroyed by your recklessness.");
        sb.Append('\n');
        return sb.ToString();
    }
}
