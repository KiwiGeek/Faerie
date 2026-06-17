using Faerie.Building;
using Faerie.Presentation;
using Faerie.Runtime;

namespace Faerie.Samples.Softporn;

/// <summary>
/// Chuck Benton's 1981 <em>Softporn Adventure</em> on the Faerie fluent engine.
/// Room text is verbatim from <c>Data/softporn.txt</c>; puzzles use <see cref="GameBuilder"/> verbs and hooks.
/// </summary>
public static class SoftpornGame
{
    public static Game Build()
    {
        GameBuilder b = GameBuilder.Create("Softporn Adventure")
            .WithMaxScore(3)
            .WithWindowTitle("Softporn Adventure")
            .WithFont("avares://Softporn/Assets/Fonts#PxPlus IBM VGA 8x16")
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
            if (yn == 'Y')
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
