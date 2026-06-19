using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

internal sealed partial class ZorkWorld
{
    private StateKey<bool> _grateRevealed = null!;

    private void DefineGrating()
    {
        _grateRevealed = _b.State("grate-revealed", false);

        Grating.PassObjectsTo(
            GratingRoom,
            maxSize: 20,
            tooLargeMessage: "It won't fit through the grating.",
            successMessage: null);

        GratingRoom.LitWhen(ctx => Grating.Has(Attr.Open));

        Clearing1.OnEnter = ctx =>
        {
            if (!ctx.Get(_grateRevealed))
                Grating.Set(Attr.Concealed);
        };

        Clearing1.Describe(ClearingLook);
        GratingRoom.Describe(GratingRoomLook);

        _b.On(Leaves).Before(_move, ctx =>
        {
            if (ctx.Get(_grateRevealed)) return VerbResult.Pass;
            ctx.Set(_grateRevealed, true);
            Grating.Set(Attr.Concealed, false);
            ctx.Say("In disturbing the pile of leaves, a grating is revealed.");
            return VerbResult.Done;
        });

        _b.On(Grating).Before(_b.Verbs.Unlock!, ctx =>
        {
            if (ctx.InRoom(Clearing1))
            {
                ctx.Say("You can't reach the lock from here.");
                return VerbResult.Done;
            }
            return VerbResult.Pass;
        });

        _b.On(Grating).Before(_b.Verbs.Lock!, ctx =>
        {
            if (ctx.InRoom(Clearing1))
            {
                ctx.Say("You can't lock it from this side.");
                return VerbResult.Done;
            }
            return VerbResult.Pass;
        });

        _b.On(Grating).Before(_b.Verbs.Open!, ctx =>
        {
            if (Grating.Has(Attr.Locked))
            {
                ctx.Say("The grating is locked.");
                return VerbResult.Done;
            }
            return VerbResult.Pass;
        });

        _b.On(Grating).After(_b.Verbs.Open!, ctx =>
        {
            if (ctx.InRoom(GratingRoom))
            {
                if (!ctx.Get(_grateRevealed))
                {
                    ctx.Set(_grateRevealed, true);
                    ctx.Move(Leaves, Placement.InRoom(GratingRoom));
                    ctx.Say("A pile of leaves falls onto your head and to the ground.");
                }
                ctx.Say("The grating opens to reveal trees above you.");
            }
            else if (ctx.InRoom(Clearing1))
            {
                ctx.Say("The grating opens.");
            }
            return VerbResult.Pass;
        });

        _b.On(Grating).After(_b.Verbs.Close!, ctx =>
        {
            if (ctx.InRoom(Clearing1) || ctx.InRoom(GratingRoom))
                ctx.Say("The grating is closed.");
            return VerbResult.Pass;
        });
    }

    private string ClearingLook(GameContext ctx)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("You are in a clearing, with a forest surrounding you on all sides. A path leads south.");
        if (Grating.Has(Attr.Open))
            sb.Append("\nThere is an open grating, descending into darkness.");
        else if (ctx.Get(_grateRevealed))
            sb.Append("\nThere is a grating securely fastened into the ground.");
        sb.Append('\n');
        return sb.ToString();
    }

    private string GratingRoomLook(GameContext ctx)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("You are in a small room near the maze. There are twisty passages in the immediate vicinity.");
        if (Grating.Has(Attr.Open))
            sb.Append("\nAbove you is an open grating with sunlight pouring in.");
        else if (!Grating.Has(Attr.Locked))
            sb.Append("\nAbove you is a grating.");
        else
            sb.Append("\nAbove you is a grating locked with a skull-and-crossbones lock.");
        sb.Append('\n');
        return sb.ToString();
    }
}
