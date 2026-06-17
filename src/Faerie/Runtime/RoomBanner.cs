using Faerie.Model;
using Faerie.Parsing;
using Faerie.Presentation;

namespace Faerie.Runtime;

/// <summary>Prints compact per-turn room summaries for Sierra-style games.</summary>
internal static class RoomBanner
{
    public static void PrintSierra(GameContext context, OutputWriter output, int separatorWidth)
    {
        GameState state = context.State;
        Room room = state.CurrentRoom;
        Scope scope = new(state, context);

        output.PrintLine(room.ResolveShortTitle(context));

        if (scope.IsLit(room))
        {
            List<string> items = scope.VisibleThings()
                .Distinct()
                .Where(t => state.RoomOf(t) == room && !t.Has(Attr.Scenery))
                .Select(t => t.Name)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();

            output.PrintLine(items.Count > 0
                ? $"Items in sight: {string.Join(", ", items)}"
                : "Items in sight: none");

            List<string> areas = [];
            foreach (Direction direction in Enum.GetValues<Direction>())
            {
                if (!room.Exits.TryGetValue(direction, out Exit? exit)) continue;
                if (!exit.CanPass(context, out _)) continue;
                areas.Add(exit.Destination.ResolveShortTitle(context));
            }

            output.PrintLine(areas.Count > 0
                ? $"Other areas: {string.Join(", ", areas)}"
                : "Other areas: none");
        }

        int width = Math.Max(1, separatorWidth);
        output.PrintLine(new string('=', width));
    }
}
