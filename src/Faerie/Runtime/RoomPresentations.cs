using Faerie.Model;
using Faerie.Parsing;
using Faerie.Presentation;

namespace Faerie.Runtime;

/// <summary>Built-in <see cref="RoomPresentation"/> presets.</summary>
public static class RoomPresentations
{
    /// <summary>
    /// Sierra / Hi-Res Adventure layout: long prose on first enter and look, compact banner each turn.
    /// </summary>
    public static RoomPresentation Sierra(SierraRoomPresentationOptions? options = null)
    {
        options ??= new();
        return new RoomPresentation
        {
            InputPrompt = options.Prompt,
            DescribeRoom = ctx => DescribeSierra(ctx),
            RefreshRoomDisplay = ctx => RoomBanner.PrintSierra(ctx, ctx.Out, ResolveSeparatorWidth(ctx, options))
        };
    }

    internal static void DescribeSierra(RoomDescribeContext ctx)
    {
        if (ctx.Moment is RoomDescribeMoment.ReEnter)
            return;

        Scope scope = new(ctx.State, ctx.Context);
        if (scope.IsLit(ctx.Room))
            ctx.Out.PrintLine(ctx.Room.ResolveDescription(ctx.Context));
        else
            ctx.Out.PrintLine("It is pitch dark, and you can see nothing.");
    }

    private static int ResolveSeparatorWidth(GameContext ctx, SierraRoomPresentationOptions options)
    {
        if (options.SeparatorWidth > 0)
            return options.SeparatorWidth;
        return Math.Max(1, ctx.Engine.Terminal.Columns);
    }
}
