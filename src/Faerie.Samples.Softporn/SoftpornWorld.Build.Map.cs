using Faerie.Building;
using Faerie.Model;

namespace Faerie.Samples.Softporn;

internal sealed partial class SoftpornWorld
{
    private void DefineRooms()
    {
        void AddRoom(string id, int messageNumber, string shortTitle) =>
            _rooms[id] = _b.Room(shortTitle)
                .ShortTitle(shortTitle)
                .Describe(SoftpornMessages.Text(messageNumber));

        // Bar
        AddRoom(SoftpornIds.Hallway, 1, "I'm in a Hallway");
        AddRoom(SoftpornIds.Bathroom, 2, "I'm in a Bathroom");
        AddRoom(SoftpornIds.Bar, 3, "I'm in a Sleazy Bar");
        AddRoom(SoftpornIds.BarStreet, 4, "I'm on a Street outside the Bar");
        AddRoom(SoftpornIds.Backroom, 5, "I'm in the Backroom");
        AddRoom(SoftpornIds.Dumpster, 6, "I'm in a Filthy Dumpster");
        AddRoom(SoftpornIds.BrokenRoom, 7, "I'm inside the room I broke into!");
        AddRoom(SoftpornIds.WindowLedge, 8, "I'm on a Window Ledge");
        AddRoom(SoftpornIds.HookerBedroom, 9, "I'm in a Hooker's Bedroom");
        AddRoom(SoftpornIds.HookerBalcony, 10, "I'm on a Hooker's Balcony");

        // Casino
        AddRoom(SoftpornIds.CasinoStreet, 11, "I'm on a Downtown Street");
        AddRoom(SoftpornIds.MarriageCenter, 12, "I'm in a Quickie Marriage Center");
        AddRoom(SoftpornIds.Casino, 13, "I'm in the Main Casino Room");
        AddRoom(SoftpornIds.TwentyOneRoom, 14, "I'm in the '21' Room");
        AddRoom(SoftpornIds.Lobby, 15, "I'm in the Lobby of the Hotel");
        AddRoom(SoftpornIds.HoneymoonSuite, 16, "I'm in the Honeymoon Suite");
        AddRoom(SoftpornIds.HotelHallway, 17, "I'm in the Hotel Hallway");
        AddRoom(SoftpornIds.HoneymoonBalcony, 18, "I'm on the Honeymooner's Balcony");
        AddRoom(SoftpornIds.HotelDesk, 19, "I'm at the Hotel Desk");

        // Disco
        AddRoom(SoftpornIds.PhoneBooth, 20, "I'm in a Telephone Booth");
        AddRoom(SoftpornIds.Disco, 21, "I'm in the Disco");
        AddRoom(SoftpornIds.DiscoStreet, 22, "I'm on a Residential Street");
        AddRoom(SoftpornIds.DiscoEntrance, 23, "I'm in the Disco's Entrance");
        AddRoom(SoftpornIds.Pharmacy, 24, "I'm in the Pharmacy");

        // Penthouse
        AddRoom(SoftpornIds.PenthouseFoyer, 25, "I'm in the Penthouse Foyer");
        AddRoom(SoftpornIds.Jacuzzi, 26, "I'm in the Jacuzzi");
        AddRoom(SoftpornIds.Kitchen, 27, "I'm in the Kitchen");
        AddRoom(SoftpornIds.Garden, 28, "I'm in the Garden");
        AddRoom(SoftpornIds.LivingRoom, 29, "I'm in the Living Room");
        AddRoom(SoftpornIds.PenthousePorch, 30, "I'm on the Penthouse Porch");
    }

    private void ConnectMap()
    {
        Link(SoftpornIds.Hallway, Direction.North, SoftpornIds.Bathroom);
        Link(SoftpornIds.Hallway, Direction.East, SoftpornIds.Bar);
        Link(SoftpornIds.Bathroom, Direction.South, SoftpornIds.Hallway);
        Link(SoftpornIds.Bar, Direction.North, SoftpornIds.BarStreet);
        Link(SoftpornIds.Bar, Direction.West, SoftpornIds.Hallway);
        LinkOneWay(SoftpornIds.Bar, Direction.East, SoftpornIds.Backroom);
        Link(SoftpornIds.BarStreet, Direction.South, SoftpornIds.Bar);
        Link(SoftpornIds.Backroom, Direction.West, SoftpornIds.Bar);
        Link(SoftpornIds.Backroom, Direction.Up, SoftpornIds.HookerBedroom);
        Link(SoftpornIds.Dumpster, Direction.West, SoftpornIds.BarStreet);
        Link(SoftpornIds.BrokenRoom, Direction.North, SoftpornIds.WindowLedge);
        LinkOneWay(SoftpornIds.WindowLedge, Direction.South, SoftpornIds.BrokenRoom);
        Link(SoftpornIds.WindowLedge, Direction.East, SoftpornIds.HookerBalcony);
        Link(SoftpornIds.HookerBedroom, Direction.North, SoftpornIds.HookerBalcony);
        Link(SoftpornIds.HookerBedroom, Direction.Down, SoftpornIds.Backroom);
        Link(SoftpornIds.HookerBalcony, Direction.South, SoftpornIds.HookerBedroom);
        Link(SoftpornIds.HookerBalcony, Direction.West, SoftpornIds.WindowLedge);
        Link(SoftpornIds.HookerBalcony, Direction.Down, SoftpornIds.Dumpster);

        Link(SoftpornIds.CasinoStreet, Direction.North, SoftpornIds.MarriageCenter);
        Link(SoftpornIds.CasinoStreet, Direction.East, SoftpornIds.Casino);
        Link(SoftpornIds.MarriageCenter, Direction.South, SoftpornIds.CasinoStreet);
        Link(SoftpornIds.Casino, Direction.North, SoftpornIds.TwentyOneRoom);
        Link(SoftpornIds.Casino, Direction.East, SoftpornIds.Lobby);
        Link(SoftpornIds.Casino, Direction.West, SoftpornIds.CasinoStreet);
        Link(SoftpornIds.TwentyOneRoom, Direction.South, SoftpornIds.Casino);
        Link(SoftpornIds.Lobby, Direction.West, SoftpornIds.Casino);
        Link(SoftpornIds.Lobby, Direction.Up, SoftpornIds.HotelDesk);
        Link(SoftpornIds.HoneymoonSuite, Direction.North, SoftpornIds.HotelHallway);
        Link(SoftpornIds.HoneymoonSuite, Direction.East, SoftpornIds.HoneymoonBalcony);
        Link(SoftpornIds.HotelHallway, Direction.East, SoftpornIds.HotelDesk);
        LinkOneWay(SoftpornIds.HotelHallway, Direction.South, SoftpornIds.HoneymoonSuite);
        Link(SoftpornIds.HoneymoonBalcony, Direction.West, SoftpornIds.HoneymoonSuite);
        Link(SoftpornIds.HotelDesk, Direction.West, SoftpornIds.HotelHallway);
        Link(SoftpornIds.HotelDesk, Direction.Down, SoftpornIds.Lobby);

        Link(SoftpornIds.PhoneBooth, Direction.North, SoftpornIds.Disco);
        Link(SoftpornIds.Disco, Direction.South, SoftpornIds.PhoneBooth);
        Link(SoftpornIds.Disco, Direction.East, SoftpornIds.DiscoEntrance);
        Link(SoftpornIds.DiscoStreet, Direction.North, SoftpornIds.DiscoEntrance);
        Link(SoftpornIds.DiscoStreet, Direction.East, SoftpornIds.Pharmacy);
        Link(SoftpornIds.DiscoEntrance, Direction.South, SoftpornIds.DiscoStreet);
        LinkOneWay(SoftpornIds.DiscoEntrance, Direction.West, SoftpornIds.Disco);
        Link(SoftpornIds.Pharmacy, Direction.West, SoftpornIds.DiscoStreet);

        Link(SoftpornIds.PenthouseFoyer, Direction.East, SoftpornIds.Kitchen);
        Link(SoftpornIds.PenthouseFoyer, Direction.Up, SoftpornIds.LivingRoom);
        Link(SoftpornIds.Jacuzzi, Direction.Up, SoftpornIds.PenthousePorch);
        Link(SoftpornIds.Kitchen, Direction.West, SoftpornIds.PenthouseFoyer);
        Link(SoftpornIds.LivingRoom, Direction.North, SoftpornIds.PenthousePorch);
        Link(SoftpornIds.LivingRoom, Direction.Down, SoftpornIds.PenthouseFoyer);
        Link(SoftpornIds.PenthousePorch, Direction.South, SoftpornIds.LivingRoom);
        Link(SoftpornIds.PenthousePorch, Direction.Down, SoftpornIds.Jacuzzi);

        LinkOneWay(SoftpornIds.Garden, Direction.South, SoftpornIds.Lobby);
    }

    private void ConfigureDynamicExits()
    {
        // Bar curtain → backroom (opened by password on button)
        if (Bar.ExitTo(Direction.East) is { } barEast)
        {
            barEast.Condition = ctx => ctx.Get(_curtainOpen);
            barEast.BlockedMessage = "I can't go that way!";
        }

        // Hooker blocks north exit until score 1
        if (HookerBedroom.ExitTo(Direction.North) is { } hookerNorth)
        {
            hookerNorth.Condition = ctx => ctx.Get(_hookerFucked);
            hookerNorth.BlockedMessage = "The Hooker says: 'Don't go there ... do me first!!'";
        }

        // Honeymoon suite door locked until married
        if (HotelHallway.ExitTo(Direction.South) is { } hallSouth)
        {
            hallSouth.Condition = ctx => ctx.Get(_marriedToGirl);
            hallSouth.BlockedMessage = "The door is locked shut!";
        }

        // Disco west door
        if (DiscoEntrance.ExitTo(Direction.West) is { } discoWest)
        {
            discoWest.Condition = ctx => ctx.Get(_doorWestOpen);
            discoWest.BlockedMessage = "The door is closed!";
        }

        // Balcony west ledge needs rope
        if (HookerBalcony.ExitTo(Direction.West) is { } balcWest)
        {
            balcWest.Condition = ctx => ctx.Get(_ropeInUse);
            balcWest.BlockedMessage = null; // fall handled in Go
        }

        // Window ledge south into broken room (after smashing window)
        if (R(SoftpornIds.WindowLedge).ExitTo(Direction.South) is { } ledgeSouth)
        {
            ledgeSouth.Condition = ctx => ctx.Get(_windowBroken);
            ledgeSouth.BlockedMessage = "I can't go that way!";
        }
    }

    private void Link(string from, Direction dir, string to, bool reciprocal = true) =>
        R(from).Connect(dir, R(to), reciprocal);

    private void LinkOneWay(string from, Direction dir, string to) =>
        R(from).Connect(dir, R(to), reciprocal: false);
}
