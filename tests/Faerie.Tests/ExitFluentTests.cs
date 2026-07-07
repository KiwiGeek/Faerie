using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;
using Xunit;

namespace Faerie.Tests;

public sealed class ExitFluentTests
{
    [Fact]
    public void DefineVerb_SameId_ReplacesEarlierHandler()
    {
        GameBuilder b = GameBuilder.Create("Replace").AddMovement();
        Room hall = b.Room("Hall");
        Room closet = b.Room("Closet");
        hall.East(closet);
        b.StartIn(hall);

        b.DefineVerb(StandardVerbIds.Go, ["go"], VerbForms.Intransitive, ctx =>
        {
            ctx.Say("CUSTOM");
            return VerbResult.Done;
        });

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.Submit("go east");

        Assert.Contains("CUSTOM", term.Output);
        Assert.Equal(hall, engine.State.CurrentRoom);
    }

    [Fact]
    public void Gate_ReturnsDynamicBlockedMessage()
    {
        GameBuilder b = GameBuilder.Create("Gate").AddMovement();
        StateKey<int> channel = b.State("channel", 0);
        Room back = b.Room("Backroom");
        Room up = b.Room("Upstairs");
        back.Up(up, gate: ctx => ctx.Get(channel) == 6 ? null : "The Pimp blocks my way upstairs!");
        b.StartIn(back);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.Submit("go up");

        Assert.Contains("Pimp blocks", term.Output);

        engine.State.Set(channel, 6);
        engine.Submit("go up");
        Assert.Equal(up, engine.State.CurrentRoom);
    }

    [Fact]
    public void OnPass_RunsBeforeMove()
    {
        GameBuilder b = GameBuilder.Create("Pay").AddMovement();
        StateKey<int> paid = b.State("paid", 0);
        Room a = b.Room("A");
        Room bRoom = b.Room("B");
        a.East(bRoom, gate: _ => null, onPass: ctx => { ctx.Set(paid, ctx.Get(paid) + 1); return true; });
        b.StartIn(a);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        engine.Submit("go east");

        Assert.Equal(1, engine.State.Get(paid));
        Assert.Equal(bRoom, engine.State.CurrentRoom);
    }

    [Fact]
    public void Reciprocal_CopiesGateAndOnPass()
    {
        GameBuilder b = GameBuilder.Create("Recip").AddMovement();
        StateKey<bool> open = b.State("open", false);
        StateKey<bool> touched = b.State("touched", false);
        Room bar = b.Room("Bar");
        Room back = b.Room("Backroom");
        back.West(bar,
            gate: ctx => ctx.CurrentRoom == back ? null : (ctx.Get(open) ? null : "I can't go that way!"),
            onPass: ctx => { ctx.Set(touched, true); return true; });
        b.StartIn(bar);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.Submit("go east");

        Assert.Contains("can't go", term.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(bar, engine.State.CurrentRoom);
        Assert.False(engine.State.Get(touched));

        engine.State.Set(open, true);
        engine.Submit("go east");
        Assert.Equal(back, engine.State.CurrentRoom);
        Assert.True(engine.State.Get(touched));
    }

    [Fact]
    public void East_When_BlocksUntilCondition()
    {
        GameBuilder b = GameBuilder.Create("Gate").AddMovement();
        StateKey<bool> open = b.State("open", false);
        Room bar = b.Room("Bar");
        Room back = b.Room("Backroom");
        bar.East(back, when: ctx => ctx.Get(open), blocked: "I can't go that way!");
        b.StartIn(bar);

        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        engine.Submit("go east");

        Assert.Contains("can't go", term.Output, StringComparison.OrdinalIgnoreCase);

        engine.State.Set(open, true);
        engine.Submit("go east");
        Assert.Equal(back, engine.State.CurrentRoom);
    }

    [Fact]
    public void Room_ExplicitId_RegistersWithGivenId()
    {
        GameBuilder b = GameBuilder.Create("Ids");
        Room hall = b.Room("hall", "Entrance Hall");
        b.StartIn(hall);

        Assert.Equal("hall", hall.Id);
        Assert.Equal("Entrance Hall", hall.Name);
        Assert.Same(hall, b.World.FindRoom("hall"));
    }

    [Fact]
    public void Thing_StartsIn_PlacesAtGameStart()
    {
        GameBuilder b = GameBuilder.Create("Place").AddCoreVerbs();
        Room kitchen = b.Room("Kitchen");
        Thing key = b.Item("key").StartsIn(kitchen);
        b.StartIn(kitchen);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();

        Assert.Same(kitchen, engine.State.PlacementOf(key).Room);
    }

    [Fact]
    public void RoomRef_ResolvesWhenDestinationRegistered()
    {
        GameBuilder b = GameBuilder.Create("Ref").AddMovement();
        RoomRef backRef = b.RoomRef("backroom");

        Room bar = b.Register(new Room("bar", "Bar"));
        bar.East(backRef, when: ctx => true);

        Room back = b.Register(new Room("backroom", "Backroom"));
        b.StartIn(bar);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        engine.Submit("go east");

        Assert.Equal(back, engine.State.CurrentRoom);
        Assert.NotNull(back.ExitTo(Direction.West));
    }

    [Fact]
    public void Build_ThrowsWhenRoomRefUnresolved()
    {
        GameBuilder b = GameBuilder.Create("Ref").AddMovement();
        Room bar = b.Register(new Room("bar", "Bar"));
        bar.East(b.RoomRef("missing"));

        Assert.Throws<InvalidOperationException>(() => b.Build());
    }

    [Fact]
    public void Exit_When_StateKeyOverload()
    {
        GameBuilder b = GameBuilder.Create("Key").AddMovement();
        StateKey<bool> open = b.State("open", false);
        Room a = b.Room("A");
        Room bRoom = b.Room("B");
        a.Connect(Direction.North, bRoom).When(open, "Blocked.");
        b.StartIn(a);

        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        engine.Submit("go north");
        Assert.Contains("Blocked", ((InMemoryTerminal)engine.Terminal).Output);
    }
}
