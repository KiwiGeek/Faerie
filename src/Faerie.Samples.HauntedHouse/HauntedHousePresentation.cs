using Faerie.Model;
using Faerie.Parsing;
using Faerie.Presentation;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.HauntedHouse;

/// <summary>Usborne-style room display: one location line, no separate prose description.</summary>
internal static class HauntedHousePresentation
{
    internal static void Describe(RoomDescribeContext ctx)
    {
        Room room = ctx.Room;
        Scope scope = new(ctx.State, ctx.Context);
        ctx.Out.Blank();

        if (!scope.IsLit(room))
        {
            ctx.Out.PrintLine("It is pitch dark, and you can see nothing.");
            return;
        }

        ctx.Out.PrintLine($"{{bold}}{{fg:white}}{room.Name}{{/}}{{/}}");

        if (ctx.Moment is not RoomDescribeMoment.ReEnter)
        {
            string flavor = room.ResolveDescription(ctx.Context);
            if (!string.IsNullOrEmpty(flavor))
                ctx.Out.PrintLine($"{{fg:gray}}{flavor}{{/}}");
        }

        DescribeContents(ctx);
        DescribeExits(ctx.Out, room);
    }

    private static void DescribeContents(RoomDescribeContext ctx)
    {
        List<Thing> here = ctx.State.ContentsOf(ctx.Room)
            .Where(t => !t.Has(Attr.Concealed) && !t.Has(Attr.Unlisted) && !t.Has(Attr.Scenery) && !t.Has(Attr.Animate))
            .ToList();

        foreach (Thing thing in here)
            ctx.Out.PrintLine($"You can see {VerbText.A(thing)} here.");

        foreach (Thing creature in ctx.State.ContentsOf(ctx.Room)
            .Where(t => !t.Has(Attr.Concealed) && !t.Has(Attr.Unlisted) && t.Has(Attr.Animate)))
            ctx.Out.PrintLine($"You can see {VerbText.A(creature)} here.");
    }

    private static void DescribeExits(OutputWriter output, Room room)
    {
        if (room.Exits.Count == 0) return;

        string exits = JoinWithAnd(room.Exits.Keys.Select(d => d.ToDisplayString()));
        output.PrintLine($"{{fg:darkgray}}Exits: {exits}.{{/}}");
    }

    private static string JoinWithAnd(IEnumerable<string> items)
    {
        List<string> list = items.ToList();
        return list.Count switch
        {
            0 => "",
            1 => list[0],
            2 => $"{list[0]} and {list[1]}",
            _ => string.Join(", ", list.Take(list.Count - 1)) + $", and {list[^1]}"
        };
    }
}
