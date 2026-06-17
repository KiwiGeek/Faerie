using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;

namespace Faerie.Samples.SierraDemo;

/// <summary>
/// Manual test bed for <see cref="RoomPresentations.Sierra"/> / <see cref="GameBuilder.WithSierraRoomBanner"/>.
/// </summary>
public static class SierraDemoGame
{
    public static Game Build()
    {
        GameBuilder b = GameBuilder.Create("Sierra Room Presentation Demo")
            .By("Faerie engine sample")
            .WithWindowTitle("Sierra Room Presentation Demo")
            .AddStandardVerbs()
            .WithSierraRoomBanner()
            .WithDefaultTitleBar();

        Room bar = b.Room("The Bar")
            .ShortTitle("BAR")
            .Describe("You are in a dim, smoky bar. A long mirror runs behind rows of bottles. " +
                      "Regulars hunch over their drinks while a jukebox mutters old hits in the corner.");

        Room back = b.Room("Back Room")
            .ShortTitle("BACK ROOM")
            .Describe("A cramped back room lit by a bare bulb. Card tables and empty chairs suggest " +
                      "after-hours business you would rather not ask about.");

        Room alley = b.Room("Alley")
            .ShortTitle("ALLEY")
            .Describe("A narrow alley behind the bar. Dumpsters, fire escapes, and the smell of stale beer.");

        Room restroom = b.Room("Men's Room")
            .ShortTitle("MEN'S ROOM")
            .Describe("Fluorescent tubes buzz over cracked tiles. The mirror is tagged with phone numbers " +
                      "you hope are fake.")
            .Dark();

        bar.East(back);
        back.East(alley);
        bar.West(restroom);

        b.Item("beer").Describe("A frosty mug of draft beer.").StartsIn(bar);
        b.Scenery("jukebox").Describe("A battered jukebox stuck on track seven.").StartsIn(bar);

        b.Creature("bartender")
            .Describe("A bored bartender polishes the same glass he has been polishing all night.")
            .StartsIn(bar);

        Thing door = b.Scenery("door").Describe("A heavy door marked EMPLOYEES ONLY.")
            .Openable(open: false);
        door.StartsIn(bar);
        AttachDoor(bar, Direction.East, door);

        b.Item("lantern").Describe("A small battery lantern.").LightSource(lit: true).StartsIn(back);

        b.WithIntro(
            "{bold}SIERRA ROOM PRESENTATION DEMO{/}\n\n" +
            "This sample uses `WithSierraRoomBanner()` — a preset over the engine's " +
            "`RoomPresentation` hooks.\n\n" +
            "Try: {fg:yellow}open door{/}, {fg:yellow}east{/}, {fg:yellow}take lantern{/}, " +
            "{fg:yellow}west{/} (dark men's room), {fg:yellow}look{/}.");

        b.StartIn(bar);
        return b.Build();
    }

    private static void AttachDoor(Room room, Direction direction, Thing door)
    {
        if (room.ExitTo(direction) is not { } exit) return;

        exit.Door = door;

        if (exit.Destination.ExitTo(direction.Opposite()) is { } back)
            back.Door = door;
    }
}
