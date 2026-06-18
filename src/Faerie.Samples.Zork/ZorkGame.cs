using Faerie.Building;
using Faerie.Model;
using Faerie.Presentation;
using Faerie.Runtime;
using Faerie.Terminal.Avalonia;

namespace Faerie.Samples.Zork;

/// <summary>
/// A Zork I-style Great Underground Empire adventure built on the fluent engine. Room layout,
/// puzzles, and treasures follow the classic game; descriptions are original prose in the same spirit.
/// </summary>
/// <remarks>
/// Several Infocom behaviors are simplified due to engine gaps. See <c>AGENTS.md</c> in this project and
/// <see cref="ZorkSimplifications"/>; puzzle code marks each shortcut with <c>ENGINE-LIMIT</c> comments.
/// </remarks>
public static class ZorkGame
{
    public static Game Build()
    {
        GameBuilder b = GameBuilder.Create("Zork I: The Great Underground Empire")
            .By("A sample for the Text Adventure engine")
            .WithMaxScore(350)
            .WithWindowTitle("Zork I — The Great Underground Empire")
            .WithFont(BuiltInTerminalFont.IbmVga8x16)
            .AddCoreVerbs()
            .AddMetaVerbs();

        ZorkWorld world = new(b);
        world.BuildAll();

        b.WithDefaultTitleBar();
        b.WithStatusBar(ctx =>
        {
            bool carryingLight = ctx.State.Inventory.Concat(ctx.State.Worn)
                .Any(t => t.Has(Attr.LightSource) && t.Has(Attr.Lit));
            string light = carryingLight ? "{fg:yellow}lit{/}" : "{fg:darkgray}dark{/}";
            return new BarContent
            {
                Left = $" {ctx.CurrentRoom.Name}",
                Right = $"{light}  F11 fullscreen ",
                Style = new TextStyle(TerminalColor.White, TerminalColor.Blue)
            };
        });

        b.WithIntro(
            "{fg:white}{bold}ZORK I: THE GREAT UNDERGROUND EMPIRE{/}{/}\n" +
            "{fg:darkgray}a classic adventure recreated for the Text Adventure engine{/}\n\n" +
            "You stand before a white house in a small clearing. Legend speaks of vast treasures hidden " +
            "in the Great Underground Empire — and of things best left undisturbed in the dark.\n\n" +
            "Type {fg:yellow}HELP{/} for commands. Deposit all 19 treasures in the trophy case, then find the secret of the stone barrow.");

        b.StartIn(world.WestOfHouse);
        return b.Build();
    }
}
