using Faerie.Building;
using Faerie.Model;
using Faerie.Parsing;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

/// <summary>Direct unit tests for <see cref="Scope.IsLit"/> / ambient <see cref="Room.IsLitFactory"/>.</summary>
public sealed class ScopeLitTests
{
    private sealed class World
    {
        internal required GameState State { get; init; }
        internal required GameContext Context { get; init; }
        internal required Room LitRoom { get; init; }
        internal required Room DarkRoom { get; init; }
        internal required Thing Lamp { get; init; }
    }

    private static World BuildWorld(Func<GameContext, bool>? darkRoomLitWhen = null)
    {
        GameBuilder b = GameBuilder.Create("LitTest").AddStandardVerbs();
        Room lit = b.Room("Lit Room").Describe("Bright.");
        Room dark = b.Room("Dark Room").Describe("Dark.").Dark();
        if (darkRoomLitWhen is not null)
            dark.LitWhen(darkRoomLitWhen);

        lit.East(dark);
        Thing lamp = b.Item("lamp").Describe("A lamp.").LightSource(lit: false);
        lamp.StartsIn(lit);
        b.StartIn(lit);

        Game game = b.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        return new World
        {
            State = engine.State,
            Context = new GameContext(engine, engine.State, engine.Out),
            LitRoom = lit,
            DarkRoom = dark,
            Lamp = lamp,
        };
    }

    private static Scope Scope(World w, bool withContext) =>
        withContext ? new Scope(w.State, w.Context) : new Scope(w.State);

    // ---- room baseline (no factory, no portable light) ------------------------------------

    [Fact]
    public void IsLit_LitRoom_IsTrue_WithoutContext()
    {
        World w = BuildWorld();
        Assert.True(Scope(w, withContext: false).IsLit(w.LitRoom));
    }

    [Fact]
    public void IsLit_DarkRoom_IsFalse_WithoutContextOrLight()
    {
        World w = BuildWorld();
        Assert.False(Scope(w, withContext: false).IsLit(w.DarkRoom));
        Assert.False(Scope(w, withContext: true).IsLit(w.DarkRoom));
    }

    [Fact]
    public void IsCurrentRoomLit_Matches_IsLit_ForCurrentRoom()
    {
        World w = BuildWorld();
        w.State.CurrentRoom = w.DarkRoom;
        Scope scope = Scope(w, withContext: true);
        Assert.Equal(scope.IsLit(w.DarkRoom), scope.IsCurrentRoomLit);
    }

    // ---- IsLitFactory (requires GameContext on Scope) -------------------------------------

    [Fact]
    public void IsLit_FactoryTrue_LightsDarkRoom_EvenWithoutPortableLight()
    {
        World w = BuildWorld(_ => true);
        Assert.True(Scope(w, withContext: true).IsLit(w.DarkRoom));
    }

    [Fact]
    public void IsLit_FactoryFalse_DarkRoomStaysDark_WithoutPortableLight()
    {
        World w = BuildWorld(_ => false);
        Assert.False(Scope(w, withContext: true).IsLit(w.DarkRoom));
    }

    [Fact]
    public void IsLit_FactoryNotConsulted_WithoutContext_EvenWhenItWouldReturnTrue()
    {
        World w = BuildWorld(_ => true);
        Assert.False(Scope(w, withContext: false).IsLit(w.DarkRoom));
    }

    [Fact]
    public void IsLit_FactoryReadsLiveState()
    {
        GameBuilder b = GameBuilder.Create("LitTest").AddStandardVerbs();
        StateKey<bool> open = b.State("open", false);
        Room dark = b.Room("Dark").Describe("Dark.").Dark().LitWhen(ctx => ctx.Get(open));
        b.StartIn(dark);
        Game game = b.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();
        GameContext ctx = new(engine, engine.State, engine.Out);
        Scope scope = new(engine.State, ctx);

        Assert.False(scope.IsLit(dark));
        engine.State.Set(open, true);
        Assert.True(scope.IsLit(dark));
        engine.State.Set(open, false);
        Assert.False(scope.IsLit(dark));
    }

    [Fact]
    public void IsLit_FactoryTrue_DoesNotAffectOtherRoom()
    {
        World w = BuildWorld(_ => true);
        Scope scope = Scope(w, withContext: true);
        Assert.True(scope.IsLit(w.DarkRoom));
        Assert.True(scope.IsLit(w.LitRoom)); // still lit via !IsDark
    }

    // ---- portable light: inventory --------------------------------------------------------

    [Fact]
    public void IsLit_CarriedLitLamp_LightsDarkRoom()
    {
        World w = BuildWorld();
        w.Lamp.Set(Attr.Lit);
        w.State.TakeIntoInventory(w.Lamp);
        Assert.True(Scope(w, withContext: true).IsLit(w.DarkRoom));
    }

    [Fact]
    public void IsLit_CarriedUnlitLamp_DoesNotLightDarkRoom()
    {
        World w = BuildWorld();
        w.State.TakeIntoInventory(w.Lamp);
        Assert.False(Scope(w, withContext: true).IsLit(w.DarkRoom));
    }

    [Fact]
    public void IsLit_LightSourceWithoutLitAttr_DoesNotCount()
    {
        World w = BuildWorld();
        w.Lamp.Set(Attr.LightSource);
        w.Lamp.Set(Attr.Lit, false);
        w.State.TakeIntoInventory(w.Lamp);
        Assert.False(Scope(w, withContext: true).IsLit(w.DarkRoom));
    }

    // ---- portable light: worn -------------------------------------------------------------

    [Fact]
    public void IsLit_WornLitLamp_LightsDarkRoom()
    {
        World w = BuildWorld();
        w.Lamp.Set(Attr.Lit);
        w.State.Move(w.Lamp, Placement.Worn);
        w.Lamp.Set(Attr.Worn);
        Assert.True(Scope(w, withContext: true).IsLit(w.DarkRoom));
    }

    // ---- in-room light (room-local) -------------------------------------------------------

    [Fact]
    public void IsLit_LitLampInDarkRoom_LightsThatRoom()
    {
        World w = BuildWorld();
        w.Lamp.Set(Attr.Lit);
        w.State.MoveTo(w.Lamp, w.DarkRoom);
        Assert.True(Scope(w, withContext: true).IsLit(w.DarkRoom));
    }

    [Fact]
    public void IsLit_LitLampInOtherRoom_DoesNotLightDarkRoom()
    {
        World w = BuildWorld();
        w.Lamp.Set(Attr.Lit);
        w.State.MoveTo(w.Lamp, w.LitRoom);
        Assert.False(Scope(w, withContext: true).IsLit(w.DarkRoom));
    }

    [Fact]
    public void IsLit_UnlitLampInDarkRoom_DoesNotLightIt()
    {
        World w = BuildWorld();
        w.State.MoveTo(w.Lamp, w.DarkRoom);
        Assert.False(Scope(w, withContext: true).IsLit(w.DarkRoom));
    }

    // ---- composition: factory vs portable light -------------------------------------------

    [Fact]
    public void IsLit_FactoryFalse_CarriedLitLamp_StillLightsDarkRoom()
    {
        World w = BuildWorld(_ => false);
        w.Lamp.Set(Attr.Lit);
        w.State.TakeIntoInventory(w.Lamp);
        Assert.True(Scope(w, withContext: true).IsLit(w.DarkRoom));
    }

    [Fact]
    public void IsLit_FactoryTrue_PreemptsDark_WithoutNeedingPortableLight()
    {
        World w = BuildWorld(_ => true);
        Assert.False(w.Lamp.Has(Attr.Lit));
        Assert.True(Scope(w, withContext: true).IsLit(w.DarkRoom));
    }

    [Fact]
    public void IsLit_LitRoom_IgnoresFactoryAndStaysLit()
    {
        World w = BuildWorld(_ => false);
        Assert.True(Scope(w, withContext: true).IsLit(w.LitRoom));
    }

    // ---- exhaustive matrix (dark room, with context) --------------------------------------

    public static TheoryData<bool?, bool, bool, bool, bool> DarkRoomMatrix()
    {
        TheoryData<bool?, bool, bool, bool, bool> data = new();
        // factoryResult (null = no factory), carriedLit, lampInDarkRoomLit, wornLit, expected
        data.Add(null, false, false, false, false);
        data.Add(null, true, false, false, true);
        data.Add(null, false, true, false, true);
        data.Add(null, false, false, true, true);
        data.Add(null, true, true, false, true);
        data.Add(false, false, false, false, false);
        data.Add(false, true, false, false, true);
        data.Add(false, false, true, false, true);
        data.Add(false, false, false, true, true);
        data.Add(true, false, false, false, true);
        data.Add(true, true, false, false, true);
        data.Add(true, false, true, false, true);
        return data;
    }

    [Theory]
    [MemberData(nameof(DarkRoomMatrix))]
    public void IsLit_DarkRoom_Matrix(
        bool? factoryResult, bool carriedLit, bool lampInDarkRoomLit, bool wornLit, bool expected)
    {
        Func<GameContext, bool>? factory = factoryResult switch
        {
            null => null,
            true => _ => true,
            false => _ => false,
        };

        World w = BuildWorld(factory);
        w.State.CurrentRoom = w.DarkRoom;

        if (lampInDarkRoomLit)
        {
            w.Lamp.Set(Attr.Lit);
            w.State.MoveTo(w.Lamp, w.DarkRoom);
        }
        else if (wornLit)
        {
            w.Lamp.Set(Attr.Lit);
            w.State.Move(w.Lamp, Placement.Worn);
            w.Lamp.Set(Attr.Worn);
        }
        else if (carriedLit)
        {
            w.Lamp.Set(Attr.Lit);
            w.State.TakeIntoInventory(w.Lamp);
        }

        Assert.Equal(expected, Scope(w, withContext: true).IsLit(w.DarkRoom));
    }
}
