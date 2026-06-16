using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Samples.Zork;
using Faerie.Verbs;
using Xunit;

namespace Faerie.Tests;

public sealed class VerbCollisionTests
{
    /// <summary>
    /// Mirrors Zork: core Push claims "move"/"push", then a game-defined move verb overrides puzzle behavior.
    /// </summary>
    private static (GameEngine engine, InMemoryTerminal term, Thing rug, Thing trapDoor) BuildRugTrapDoorPuzzle()
    {
        GameBuilder b = GameBuilder.Create("Rug").AddCoreVerbs();
        Room room = b.Room("Living Room").Describe("A living room with a rug.");
        Thing rug = b.Scenery("oriental rug").Called("rug").Describe("A heavy rug.");
        Thing trapDoor = b.Scenery("trap door").Called("door", "trapdoor").Adjectives("trap")
            .Openable(open: false).Describe("A trap door.").Concealed();
        rug.StartsIn(room);
        trapDoor.StartsIn(room);
        StateKey<bool> rugMoved = b.State("rug-moved", false);

        Verb move = b.DefineVerb("move", ["move", "push", "pull", "slide"], VerbForms.Transitive, ctx =>
        {
            if (ctx.DirectObject == rug) return VerbResult.Pass;
            ctx.Say("You can't move that.");
            return VerbResult.Done;
        });

        b.On(rug).Before(move, ctx =>
        {
            if (!ctx.Get(rugMoved))
            {
                ctx.Set(rugMoved, true);
                trapDoor.Set(Attr.Concealed, false);
                ctx.Say("With a great effort, you slide the rug aside, revealing a closed trap door.");
                return VerbResult.Done;
            }
            ctx.Say("The rug is already moved.");
            return VerbResult.Done;
        });

        b.StartIn(room);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        return (engine, term, rug, trapDoor);
    }

    [Theory]
    [InlineData("move rug")]
    [InlineData("push rug")]
    [InlineData("pull rug")]
    [InlineData("slide rug")]
    public void GameMoveVerb_OverridesCorePush_ForObjectCommands(string command)
    {
        (GameEngine engine, InMemoryTerminal term, _, Thing trapDoor) = BuildRugTrapDoorPuzzle();
        term.Reset();
        engine.Submit(command);

        Assert.Contains("revealing a closed trap door", term.Output);
        Assert.False(trapDoor.Has(Attr.Concealed));

        term.Reset();
        engine.Submit("open trap door");
        Assert.DoesNotContain("can't see any trap door", term.Output);
        Assert.Contains("open the trap door", term.Output);
    }

    [Theory]
    [InlineData("move rug")]
    [InlineData("push rug")]
    public void Zork_LivingRoom_MoveRug_RevealsTrapDoor(string command)
    {
        Game game = ZorkGame.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        Thing trapDoor = game.World.Things.First(t => t.Name == "trap door");
        Room living = game.World.Rooms.First(r => r.Name == "Living Room");

        engine.State.CurrentRoom = living;
        term.Reset();
        engine.Submit(command);

        Assert.Contains("revealing a closed trap door", term.Output);
        Assert.False(trapDoor.Has(Attr.Concealed));

        term.Reset();
        engine.Submit("open trap door");
        Assert.DoesNotContain("can't see any trap door", term.Output);
        Assert.Contains("open the trap door", term.Output);
    }
}
