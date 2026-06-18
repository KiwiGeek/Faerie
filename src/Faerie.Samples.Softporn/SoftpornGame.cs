using Faerie.Building;
using Faerie.Presentation;
using Faerie.Runtime;
using Faerie.Terminal.Avalonia;

namespace Faerie.Samples.Softporn;

/// <summary>
/// Chuck Benton's 1981 <em>Softporn Adventure</em> on the Faerie fluent engine.
/// Every room, item and message is authored directly in C# (no external data file); puzzles use
/// <see cref="GameBuilder"/> verbs and hooks.
/// </summary>
public static class SoftpornGame
{
    public static Game Build()
    {
        GameBuilder b = GameBuilder.Create("Softporn Adventure")
            .WithMaxScore(3)
            .WithWindowTitle("Softporn Adventure")
            .WithFont(BuiltInTerminalFont.IbmVga8x16)
            .AddMovement()
            .AddCoreVerbs()
            .AddMetaVerbs()
            .WithSierraRoomBanner("What shall I do? ");

        SoftpornWorld world = new(b);
        world.BuildAll();

        b.WithIntro("Welcome to SOFTPORN ADVENTURE!!");

        b.OnStart(ctx =>
        {
            ctx.Blank();
            char yn = ctx.PromptKey("Do you need instructions? (y/n) ", "YN");
            if (yn is 'Y' or 'y')
                world.ShowHelp(ctx);
            else
            {
                ctx.Blank();
                ctx.Blank();
            }
        });

        b.WithStatusBar(ctx => new BarContent
        {
            Left = $" ${ctx.Get(world.MoneyKey)}00 ",
            Right = $" Score {ctx.State.Score}/3 ",
            Style = new TextStyle(TerminalColor.White, TerminalColor.Blue)
        });

        b.StartIn(world.Bar);
        return b.Build();
    }
}
