using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class SpatialQueryTests
{
    private sealed record Fixture(
        GameContext Ctx,
        Room Hall,
        Room Cellar,
        Room Attic,
        Thing Lamp,
        Thing Key,
        Thing Chest,
        Thing Bag,
        Thing Coin,
        Thing Goblin);

    private static Fixture Build()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Room cellar = b.Room("Cellar").Describe("A cellar.");
        Room attic = b.Room("Attic").Describe("An attic.");
        hall.Down(cellar);
        hall.Up(attic);

        Thing lamp = b.Item("lamp").Describe("A lamp.");
        Thing key = b.Item("key").Describe("A key.");
        Thing chest = b.Scenery("chest").Describe("A chest.").Container(open: false);
        Thing bag = b.Item("bag").Describe("A leather bag.").Container(open: true);
        Thing coin = b.Item("coin").Describe("A gold coin.");
        Thing goblin = b.Creature("goblin").Describe("A goblin.");

        lamp.StartsIn(hall);
        chest.StartsIn(hall);
        key.StartsInside(chest);
        bag.StartsIn(hall);
        coin.StartsInside(bag);
        goblin.StartsIn(cellar);
        b.StartIn(hall);

        Game game = b.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();
        GameContext ctx = new(engine, engine.State, engine.Out);
        return new Fixture(ctx, hall, cellar, attic, lamp, key, chest, bag, coin, goblin);
    }

    [Fact]
    public void ThingsHere_ReturnsLooseContentsOfCurrentRoom()
    {
        Fixture f = Build();
        List<Thing> here = f.Ctx.ThingsHere().ToList();
        Assert.Contains(f.Lamp, here);
        Assert.Contains(f.Chest, here);
        Assert.DoesNotContain(f.Key, here);
        Assert.DoesNotContain(f.Goblin, here);
    }

    [Fact]
    public void ThingsIn_ReturnsLooseContentsOfGivenRoom()
    {
        Fixture f = Build();
        List<Thing> cellar = f.Ctx.ThingsIn(f.Cellar).ToList();
        Assert.Single(cellar);
        Assert.Contains(f.Goblin, cellar);
        Assert.DoesNotContain(f.Lamp, cellar);
    }

    [Fact]
    public void ThingsIn_IncludePresent_ContainsNestedRoomContents()
    {
        Fixture f = Build();
        List<Thing> hall = f.Ctx.ThingsIn(f.Hall, includePresent: true).ToList();
        Assert.Contains(f.Lamp, hall);
        Assert.Contains(f.Chest, hall);
        Assert.Contains(f.Key, hall);
        Assert.Contains(f.Bag, hall);
        Assert.Contains(f.Coin, hall);
        Assert.DoesNotContain(f.Goblin, hall);
    }

    [Fact]
    public void ThingsIn_IncludePresent_ContainsCarriedItemsWhenPlayerInRoom()
    {
        Fixture f = Build();
        f.Ctx.Take(f.Lamp);
        List<Thing> hall = f.Ctx.ThingsIn(f.Hall, includePresent: true).ToList();
        Assert.Contains(f.Lamp, hall);
    }

    [Fact]
    public void IsLocatedIn_ExcludesCarriedItems()
    {
        Fixture f = Build();
        Assert.True(f.Ctx.LocatedIn(f.Lamp, f.Hall));
        f.Ctx.Take(f.Lamp);
        Assert.False(f.Ctx.LocatedIn(f.Lamp, f.Hall));
        Assert.True(f.Ctx.Here(f.Lamp));
    }

    [Fact]
    public void ThingsLocatedIn_IncludesNestedContentsButNotInventory()
    {
        Fixture f = Build();
        List<Thing> hall = f.Ctx.ThingsLocatedIn(f.Hall).ToList();
        Assert.Contains(f.Lamp, hall);
        Assert.Contains(f.Key, hall);
        Assert.Contains(f.Coin, hall);
        Assert.DoesNotContain(f.Goblin, hall);

        f.Ctx.Take(f.Lamp);
        hall = f.Ctx.ThingsLocatedIn(f.Hall).ToList();
        Assert.DoesNotContain(f.Lamp, hall);
        Assert.Contains(f.Key, hall);
    }

    [Fact]
    public void ThingsIn_IncludePresent_ContainsItemsInsideCarriedContainer()
    {
        Fixture f = Build();
        f.Ctx.Take(f.Bag);
        List<Thing> hall = f.Ctx.ThingsIn(f.Hall, includePresent: true).ToList();
        Assert.Contains(f.Bag, hall);
        Assert.Contains(f.Coin, hall);
    }

    [Fact]
    public void ThingsIn_IncludePresent_ExcludesCarriedItemsAfterPlayerLeavesRoom()
    {
        Fixture f = Build();
        f.Ctx.Take(f.Lamp);
        f.Ctx.MovePlayerTo(f.Cellar);
        List<Thing> hall = f.Ctx.ThingsIn(f.Hall, includePresent: true).ToList();
        Assert.DoesNotContain(f.Lamp, hall);
    }

    [Fact]
    public void ThingsHere_IncludePresent_MatchesThingsInCurrentRoom()
    {
        Fixture f = Build();
        IEnumerable<Thing> here = f.Ctx.ThingsHere(includePresent: true);
        IEnumerable<Thing> expected = f.Ctx.ThingsIn(f.Hall, includePresent: true);
        Assert.Equal(expected.OrderBy(t => t.Id), here.OrderBy(t => t.Id));
    }

    [Fact]
    public void IsAdjacent_TrueForExitDestination()
    {
        Fixture f = Build();
        Assert.True(f.Ctx.IsAdjacent(f.Cellar));
        Assert.True(f.Ctx.IsAdjacent(f.Attic));
    }

    [Fact]
    public void IsAdjacent_FalseForSameRoomAndDistantRoom()
    {
        Fixture f = Build();
        Assert.False(f.Ctx.IsAdjacent(f.Hall));
        f.Ctx.MovePlayerTo(f.Cellar);
        Assert.False(f.Ctx.IsAdjacent(f.Attic));
    }

    [Fact]
    public void Nearby_TrueForThingInCurrentRoom()
    {
        Fixture f = Build();
        Assert.True(f.Ctx.Nearby(f.Lamp));
        Assert.True(f.Ctx.Nearby(f.Chest));
    }

    [Fact]
    public void Nearby_TrueForThingInContainerInCurrentRoom()
    {
        Fixture f = Build();
        Assert.True(f.Ctx.Nearby(f.Key));
    }

    [Fact]
    public void Nearby_TrueForThingOneExitAway()
    {
        Fixture f = Build();
        Assert.True(f.Ctx.Nearby(f.Goblin));
    }

    [Fact]
    public void Nearby_FalseForOffstageThing()
    {
        Fixture f = Build();
        f.Ctx.Remove(f.Lamp);
        Assert.False(f.Ctx.Nearby(f.Lamp));
    }

    [Fact]
    public void Nearby_FalseWhenThingIsTwoRoomsAway()
    {
        Fixture f = Build();
        f.Ctx.MovePlayerTo(f.Attic);
        Assert.False(f.Ctx.Nearby(f.Goblin));
    }

    [Fact]
    public void Nearby_TrueForCarriedThing()
    {
        Fixture f = Build();
        f.Ctx.Take(f.Lamp);
        Assert.True(f.Ctx.Nearby(f.Lamp));
    }

    [Fact]
    public void Nearby_CarriedThingStaysNearbyAfterPlayerMoves()
    {
        Fixture f = Build();
        f.Ctx.Take(f.Goblin);
        f.Ctx.MovePlayerTo(f.Cellar);
        Assert.True(f.Ctx.Nearby(f.Goblin));
    }
}
