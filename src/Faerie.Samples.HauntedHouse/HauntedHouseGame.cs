using Faerie.Building;
using Faerie.Presentation;
using Faerie.Runtime;
using Faerie.Terminal.Avalonia;

namespace Faerie.Samples.HauntedHouse;

/// <summary>
/// Usborne <em>Write Your Own Adventure Programs</em> — Haunted House, on the Faerie engine.
/// </summary>
public static class HauntedHouseGame
{
    public static Game Build()
    {
        GameBuilder b = GameBuilder.Create("Haunted House")
            .By("Jenny Tyler & Les Howarth, via the Faerie engine")
            .WithWindowTitle("Haunted House — a Text Adventure")
            .WithFont(BuiltInTerminalFont.AppleIIe)
            .WithCursor(TerminalCursor.Block)
            .AddMovement()
            .WithRoomPresentation(new RoomPresentation { DescribeRoom = HauntedHousePresentation.Describe })
            .WithDefaultTitleBar()
            .WithStatusBar(ctx => new BarContent
            {
                Left = $" {ctx.CurrentRoom.Name}",
                Right = " F11 fullscreen ",
                Style = new TextStyle(TerminalColor.White, TerminalColor.Blue)
            })
            .WithIntro(
                "{fg:white}{bold}HAUNTED HOUSE{/}{/}\n" +
                "{fg:darkgray}from Write Your Own Adventure Programs (Usborne){/}\n\n" +
                "You have to explore your way around a haunted house. In the house (or perhaps outside) " +
                "you will have to find seventeen objects and bring them back to the main gate.\n\n" +
                "Type {fg:yellow}HELP{/} at any time for commands.");

        HauntedHouseWorld world = new(b);
        world.BuildAll();

        b.StartIn(world.IronGate);

        return b.Build();
    }
}
