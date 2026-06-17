using Faerie.Building;
using Faerie.Model;
using Faerie.Parsing;
using Faerie.Runtime;
using Xunit;

namespace Faerie.Tests;

public sealed class OrderableScopeTests
{
    private static (GameEngine engine, Room shop, Thing wine, Thing vendor) BuildShop()
    {
        GameBuilder b = GameBuilder.Create("Shop").AddStandardVerbs();
        Room shop = b.Room("Shop").Describe("A shop.");
        Thing vendor = b.Creature("waitress").Called("waitress").Describe("A waitress.");
        Thing wine = b.Item("wine").Called("wine").Describe("A bottle of wine.").OrderableFrom(vendor);
        vendor.StartsIn(shop);
        b.StartIn(shop);
        InMemoryTerminal term = new();
        GameEngine engine = new(b.Build(), term, randomSeed: 1);
        engine.Start();
        return (engine, shop, wine, vendor);
    }

    [Fact]
    public void OffstageOrderable_ResolvableWhenVendorPresent()
    {
        (GameEngine engine, _, Thing wine, _) = BuildShop();
        Scope scope = new(engine.State, new GameContext(engine, engine.State, engine.Out));

        Assert.Null(engine.State.RoomOf(wine));
        Assert.Contains(wine, scope.VisibleThings());
        Assert.Equal(NounResolution.Kind.Single, scope.Resolve(["wine"]).Status);
    }

    [Fact]
    public void OffstageOrderable_NotResolvableWithoutVendor()
    {
        (GameEngine engine, _, Thing wine, Thing vendor) = BuildShop();
        engine.State.Move(vendor, Placement.Offstage);

        Scope scope = new(engine.State, new GameContext(engine, engine.State, engine.Out));
        Assert.DoesNotContain(wine, scope.VisibleThings());
        Assert.Equal(NounResolution.Kind.NotFound, scope.Resolve(["wine"]).Status);
    }

    [Fact]
    public void OffstageOrderable_NotListedInRoomBanner()
    {
        (GameEngine engine, _, Thing wine, _) = BuildShop();
        var term = (InMemoryTerminal)engine.Terminal;
        RoomBanner.PrintSierra(new GameContext(engine, engine.State, engine.Out), engine.Out, 40);

        Assert.Contains(wine, new Scope(engine.State).VisibleThings());
        Assert.DoesNotContain("wine", term.Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OrderableStock_NotResolvableWhenStockExistsElsewhere()
    {
        GameBuilder b = GameBuilder.Create("Stock").AddStandardVerbs();
        Room shop = b.Room("Shop").Describe("A shop.");
        Room cellar = b.Room("Cellar").Describe("A cellar.");
        shop.Connect(Direction.Down, cellar);
        Thing vendor = b.Creature("waitress").Called("waitress").Describe("A waitress.");
        Thing wine = b.Item("wine").Called("wine").Describe("Wine.").OrderableFrom(vendor);
        vendor.StartsIn(shop);
        b.StartIn(shop);
        GameEngine engine = new(b.Build(), new InMemoryTerminal(), randomSeed: 1);
        engine.Start();
        engine.State.Move(wine, Placement.InRoom(cellar));

        Scope scope = new(engine.State);
        Assert.DoesNotContain(wine, scope.VisibleThings());
        Assert.Equal(NounResolution.Kind.NotFound, scope.Resolve(["wine"]).Status);
    }
}
