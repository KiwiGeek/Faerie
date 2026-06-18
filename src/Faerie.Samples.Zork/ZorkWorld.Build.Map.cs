using Faerie.Building;
using Faerie.Model;

namespace Faerie.Samples.Zork;

internal sealed partial class ZorkWorld
{
    private void ConnectMap()
    {
        ConnectSurface();
        ConnectHouse();
        ConnectUpperDungeon();
        ConnectMaze();
        ConnectLowerDungeon();
        ConnectDamAndReservoir();
        ConnectRiver();
        ConnectCoalMine();
    }

    private void ConnectSurface()
    {
        Link(ZorkIds.WestOfHouse, Direction.North, ZorkIds.NorthOfHouse);
        Link(ZorkIds.WestOfHouse, Direction.South, ZorkIds.SouthOfHouse);
        Link(ZorkIds.WestOfHouse, Direction.West, ZorkIds.Forest1);
        Link(ZorkIds.WestOfHouse, Direction.NorthEast, ZorkIds.NorthOfHouse, false);
        Link(ZorkIds.WestOfHouse, Direction.SouthEast, ZorkIds.SouthOfHouse, false);
        Block(ZorkIds.WestOfHouse, Direction.East, "The door is boarded and you can't remove the boards.");

        Link(ZorkIds.NorthOfHouse, Direction.North, ZorkIds.ForestPath);
        Link(ZorkIds.NorthOfHouse, Direction.West, ZorkIds.WestOfHouse);
        Link(ZorkIds.NorthOfHouse, Direction.East, ZorkIds.EastOfHouse);
        Block(ZorkIds.NorthOfHouse, Direction.South, "The windows are all boarded.");

        Link(ZorkIds.SouthOfHouse, Direction.South, ZorkIds.Forest4);
        Link(ZorkIds.SouthOfHouse, Direction.West, ZorkIds.WestOfHouse);
        Link(ZorkIds.SouthOfHouse, Direction.East, ZorkIds.EastOfHouse);
        Link(ZorkIds.SouthOfHouse, Direction.NorthWest, ZorkIds.WestOfHouse, false);
        Link(ZorkIds.SouthOfHouse, Direction.NorthEast, ZorkIds.EastOfHouse, false);
        Block(ZorkIds.SouthOfHouse, Direction.North, "The windows are all boarded.");

        Link(ZorkIds.EastOfHouse, Direction.East, ZorkIds.Clearing2);
        Link(ZorkIds.EastOfHouse, Direction.North, ZorkIds.NorthOfHouse);
        Link(ZorkIds.EastOfHouse, Direction.South, ZorkIds.SouthOfHouse);
        Link(ZorkIds.EastOfHouse, Direction.NorthWest, ZorkIds.NorthOfHouse, false);
        Link(ZorkIds.EastOfHouse, Direction.SouthWest, ZorkIds.SouthOfHouse, false);

        Link(ZorkIds.ForestPath, Direction.North, ZorkIds.Clearing1);
        Link(ZorkIds.ForestPath, Direction.South, ZorkIds.NorthOfHouse);
        Link(ZorkIds.ForestPath, Direction.West, ZorkIds.Forest1);
        Link(ZorkIds.ForestPath, Direction.East, ZorkIds.Forest2);
        Link(ZorkIds.ForestPath, Direction.Up, ZorkIds.UpATree);

        Link(ZorkIds.Clearing1, Direction.South, ZorkIds.ForestPath);
        Link(ZorkIds.Clearing1, Direction.West, ZorkIds.Forest1);
        Link(ZorkIds.Clearing1, Direction.East, ZorkIds.Forest2);
        Block(ZorkIds.Clearing1, Direction.North, "The forest becomes impenetrable to the north.");

        Link(ZorkIds.Clearing2, Direction.North, ZorkIds.Forest2);
        Link(ZorkIds.Clearing2, Direction.South, ZorkIds.Forest4);
        Link(ZorkIds.Clearing2, Direction.West, ZorkIds.EastOfHouse);
        Link(ZorkIds.Clearing2, Direction.East, ZorkIds.CanyonView);

        Link(ZorkIds.Forest1, Direction.North, ZorkIds.Clearing1);
        Link(ZorkIds.Forest1, Direction.South, ZorkIds.Forest4);
        Link(ZorkIds.Forest1, Direction.East, ZorkIds.ForestPath);
        Block(ZorkIds.Forest1, Direction.West, "You would need a machete to go further west.");

        Link(ZorkIds.Forest2, Direction.South, ZorkIds.Clearing2);
        Link(ZorkIds.Forest2, Direction.West, ZorkIds.ForestPath);
        Link(ZorkIds.Forest2, Direction.East, ZorkIds.Forest3);
        Block(ZorkIds.Forest2, Direction.North, "The forest becomes impenetrable to the north.");

        Link(ZorkIds.Forest3, Direction.South, ZorkIds.Forest2);
        Link(ZorkIds.Forest3, Direction.West, ZorkIds.Forest2);
        Link(ZorkIds.Forest3, Direction.East, ZorkIds.Forest2);
        Link(ZorkIds.Forest3, Direction.North, ZorkIds.Forest2);
        Block(ZorkIds.Forest3, Direction.Up, "The mountains are impassable.");

        Link(ZorkIds.Forest4, Direction.North, ZorkIds.Clearing2);
        Link(ZorkIds.Forest4, Direction.West, ZorkIds.Forest1);
        Link(ZorkIds.Forest4, Direction.NorthWest, ZorkIds.SouthOfHouse);
        Block(ZorkIds.Forest4, Direction.South, "Storm-tossed trees block your way.");
        Block(ZorkIds.Forest4, Direction.East, "The rank undergrowth prevents eastward movement.");

        Link(ZorkIds.UpATree, Direction.Down, ZorkIds.ForestPath);
        Block(ZorkIds.UpATree, Direction.Up, "You cannot climb any higher.");

        Link(ZorkIds.StoneBarrow, Direction.NorthEast, ZorkIds.WestOfHouse, false);

        Link(ZorkIds.CanyonView, Direction.Down, ZorkIds.RockyLedge);
        Link(ZorkIds.CanyonView, Direction.East, ZorkIds.RockyLedge, false);
        Link(ZorkIds.CanyonView, Direction.NorthWest, ZorkIds.Clearing2);
        Link(ZorkIds.CanyonView, Direction.West, ZorkIds.Forest4);
        Block(ZorkIds.CanyonView, Direction.South, "You would fall if you tried to go that way.");

        Link(ZorkIds.RockyLedge, Direction.Up, ZorkIds.CanyonView);
        Link(ZorkIds.RockyLedge, Direction.Down, ZorkIds.CanyonBottom);

        Link(ZorkIds.CanyonBottom, Direction.Up, ZorkIds.RockyLedge);
        Link(ZorkIds.CanyonBottom, Direction.North, ZorkIds.EndOfRainbow);
    }

    private void ConnectHouse()
    {
        Link(ZorkIds.Kitchen, Direction.West, ZorkIds.LivingRoom);
        Link(ZorkIds.Kitchen, Direction.Up, ZorkIds.Attic);
        Link(ZorkIds.LivingRoom, Direction.East, ZorkIds.Kitchen);
        Link(ZorkIds.Attic, Direction.Down, ZorkIds.Kitchen);
        Link(ZorkIds.StrangePassage, Direction.East, ZorkIds.LivingRoom);
        LinkOneWay(ZorkIds.StrangePassage, Direction.West, ZorkIds.CyclopsRoom);
        LinkOneWay(ZorkIds.StrangePassage, Direction.In, ZorkIds.CyclopsRoom);
    }

    private void ConnectUpperDungeon()
    {
        Link(ZorkIds.Cellar, Direction.North, ZorkIds.TrollRoom);
        Link(ZorkIds.Cellar, Direction.South, ZorkIds.EastOfChasm);
        Block(ZorkIds.Cellar, Direction.West, "You would slide down the chute if you tried that.");

        Link(ZorkIds.EastOfChasm, Direction.North, ZorkIds.Cellar);
        Link(ZorkIds.EastOfChasm, Direction.East, ZorkIds.Gallery);
        Block(ZorkIds.EastOfChasm, Direction.Down, "The chasm probably leads straight to the infernal regions.");

        Link(ZorkIds.Gallery, Direction.West, ZorkIds.EastOfChasm);
        Link(ZorkIds.Gallery, Direction.North, ZorkIds.Studio);

        Link(ZorkIds.Studio, Direction.South, ZorkIds.Gallery);

        LinkOneWay(ZorkIds.TrollRoom, Direction.South, ZorkIds.Cellar);
        LinkOneWay(ZorkIds.TrollRoom, Direction.East, ZorkIds.EwPassage);
        LinkOneWay(ZorkIds.TrollRoom, Direction.West, ZorkIds.Maze1);

        Link(ZorkIds.EwPassage, Direction.West, ZorkIds.TrollRoom);
        Link(ZorkIds.EwPassage, Direction.East, ZorkIds.RoundRoom);
        Link(ZorkIds.EwPassage, Direction.Down, ZorkIds.Chasm);
        Link(ZorkIds.EwPassage, Direction.North, ZorkIds.Chasm, false);

        Link(ZorkIds.RoundRoom, Direction.North, ZorkIds.NsPassage);
        Link(ZorkIds.RoundRoom, Direction.East, ZorkIds.LoudRoom);
        Link(ZorkIds.RoundRoom, Direction.West, ZorkIds.EwPassage);
        Link(ZorkIds.RoundRoom, Direction.South, ZorkIds.NarrowPassage);
        Link(ZorkIds.RoundRoom, Direction.SouthEast, ZorkIds.EngravingsCave);

        Link(ZorkIds.EngravingsCave, Direction.NorthWest, ZorkIds.RoundRoom);
        Link(ZorkIds.EngravingsCave, Direction.East, ZorkIds.DomeRoom);

        Link(ZorkIds.DomeRoom, Direction.West, ZorkIds.EngravingsCave);

        Link(ZorkIds.LoudRoom, Direction.West, ZorkIds.RoundRoom);
        Link(ZorkIds.LoudRoom, Direction.East, ZorkIds.DampCave);
        Link(ZorkIds.LoudRoom, Direction.Up, ZorkIds.DeepCanyon);

        Link(ZorkIds.DeepCanyon, Direction.East, ZorkIds.Dam);
        Link(ZorkIds.DeepCanyon, Direction.NorthWest, ZorkIds.ReservoirSouth);
        Link(ZorkIds.DeepCanyon, Direction.SouthWest, ZorkIds.NsPassage);
        Link(ZorkIds.DeepCanyon, Direction.Down, ZorkIds.LoudRoom);

        LinkOneWay(ZorkIds.GratingRoom, Direction.SouthWest, ZorkIds.Maze13);
    }

    private void ConnectMaze()
    {
        LinkOneWay(ZorkIds.Maze1, Direction.North, ZorkIds.Maze1);
        LinkOneWay(ZorkIds.Maze1, Direction.South, ZorkIds.Maze4);
        LinkOneWay(ZorkIds.Maze1, Direction.West, ZorkIds.Maze2);
        LinkOneWay(ZorkIds.Maze1, Direction.East, ZorkIds.TrollRoom);

        LinkOneWay(ZorkIds.Maze2, Direction.North, ZorkIds.Maze1);
        LinkOneWay(ZorkIds.Maze2, Direction.West, ZorkIds.Maze3);
        LinkOneWay(ZorkIds.Maze2, Direction.East, ZorkIds.Maze19);

        LinkOneWay(ZorkIds.Maze3, Direction.North, ZorkIds.Maze2);
        LinkOneWay(ZorkIds.Maze3, Direction.West, ZorkIds.Maze4);
        LinkOneWay(ZorkIds.Maze3, Direction.Up, ZorkIds.Maze5);

        LinkOneWay(ZorkIds.Maze4, Direction.South, ZorkIds.Maze1);
        LinkOneWay(ZorkIds.Maze4, Direction.East, ZorkIds.Maze3);
        LinkOneWay(ZorkIds.Maze4, Direction.Down, ZorkIds.Maze2);

        LinkOneWay(ZorkIds.Maze5, Direction.North, ZorkIds.Maze3);
        LinkOneWay(ZorkIds.Maze5, Direction.SouthWest, ZorkIds.Maze6);
        LinkOneWay(ZorkIds.Maze5, Direction.East, ZorkIds.Maze18);

        LinkOneWay(ZorkIds.Maze6, Direction.Down, ZorkIds.Maze5);
        LinkOneWay(ZorkIds.Maze6, Direction.Up, ZorkIds.Maze15);
        LinkOneWay(ZorkIds.Maze6, Direction.West, ZorkIds.Maze6);
        LinkOneWay(ZorkIds.Maze6, Direction.East, ZorkIds.Maze7);

        LinkOneWay(ZorkIds.Maze7, Direction.Down, ZorkIds.Maze19);
        LinkOneWay(ZorkIds.Maze7, Direction.Up, ZorkIds.Maze9);
        LinkOneWay(ZorkIds.Maze7, Direction.South, ZorkIds.Maze8);
        LinkOneWay(ZorkIds.Maze7, Direction.West, ZorkIds.Maze6);
        LinkOneWay(ZorkIds.Maze7, Direction.East, ZorkIds.Maze17);

        LinkOneWay(ZorkIds.Maze8, Direction.SouthEast, ZorkIds.CyclopsRoom);
        LinkOneWay(ZorkIds.Maze8, Direction.South, ZorkIds.Maze7);
        LinkOneWay(ZorkIds.Maze8, Direction.West, ZorkIds.Maze9);

        LinkOneWay(ZorkIds.Maze9, Direction.NorthWest, ZorkIds.Maze9);
        LinkOneWay(ZorkIds.Maze9, Direction.NorthEast, ZorkIds.Maze7);
        LinkOneWay(ZorkIds.Maze9, Direction.South, ZorkIds.Maze7);
        LinkOneWay(ZorkIds.Maze9, Direction.West, ZorkIds.Maze8);

        LinkOneWay(ZorkIds.Maze10, Direction.Down, ZorkIds.Maze12);
        LinkOneWay(ZorkIds.Maze10, Direction.South, ZorkIds.Maze14);
        LinkOneWay(ZorkIds.Maze10, Direction.West, ZorkIds.Maze13);
        LinkOneWay(ZorkIds.Maze10, Direction.East, ZorkIds.Maze15);

        LinkOneWay(ZorkIds.Maze11, Direction.South, ZorkIds.Maze12);

        LinkOneWay(ZorkIds.Maze12, Direction.Down, ZorkIds.Maze5);
        LinkOneWay(ZorkIds.Maze12, Direction.Up, ZorkIds.Maze15);
        LinkOneWay(ZorkIds.Maze12, Direction.SouthWest, ZorkIds.Maze13);
        LinkOneWay(ZorkIds.Maze12, Direction.East, ZorkIds.Maze10);
        LinkOneWay(ZorkIds.Maze12, Direction.North, ZorkIds.Maze11);

        LinkOneWay(ZorkIds.Maze13, Direction.Down, ZorkIds.Maze14);
        LinkOneWay(ZorkIds.Maze13, Direction.SouthWest, ZorkIds.Maze12);
        LinkOneWay(ZorkIds.Maze13, Direction.NorthWest, ZorkIds.Maze10);
        LinkOneWay(ZorkIds.Maze13, Direction.NorthEast, ZorkIds.GratingRoom);

        LinkOneWay(ZorkIds.Maze14, Direction.Up, ZorkIds.Maze13);
        LinkOneWay(ZorkIds.Maze14, Direction.West, ZorkIds.Maze10);
        LinkOneWay(ZorkIds.Maze14, Direction.East, ZorkIds.Maze15);

        LinkOneWay(ZorkIds.Maze15, Direction.Down, ZorkIds.Maze13);
        LinkOneWay(ZorkIds.Maze15, Direction.NorthWest, ZorkIds.Maze15);
        LinkOneWay(ZorkIds.Maze15, Direction.South, ZorkIds.Maze10);
        LinkOneWay(ZorkIds.Maze15, Direction.West, ZorkIds.Maze12);
        LinkOneWay(ZorkIds.Maze15, Direction.East, ZorkIds.Maze14);
        LinkOneWay(ZorkIds.Maze15, Direction.North, ZorkIds.Maze6);

        LinkOneWay(ZorkIds.Maze16, Direction.North, ZorkIds.Maze17);

        LinkOneWay(ZorkIds.Maze17, Direction.SouthEast, ZorkIds.Maze16);
        LinkOneWay(ZorkIds.Maze17, Direction.NorthEast, ZorkIds.Maze7);
        LinkOneWay(ZorkIds.Maze17, Direction.West, ZorkIds.Maze17);

        LinkOneWay(ZorkIds.Maze18, Direction.West, ZorkIds.Maze5);
        LinkOneWay(ZorkIds.Maze19, Direction.South, ZorkIds.Maze2);
    }

    private void ConnectLowerDungeon()
    {
        LinkOneWay(ZorkIds.CyclopsRoom, Direction.NorthWest, ZorkIds.Maze8);
        LinkOneWay(ZorkIds.CyclopsRoom, Direction.Up, ZorkIds.TreasureRoom);
        LinkOneWay(ZorkIds.CyclopsRoom, Direction.East, ZorkIds.StrangePassage);

        LinkOneWay(ZorkIds.TreasureRoom, Direction.Down, ZorkIds.CyclopsRoom);
        LinkOneWay(ZorkIds.TreasureRoom, Direction.West, ZorkIds.MirrorRoom2);

        Link(ZorkIds.NsPassage, Direction.North, ZorkIds.Chasm);
        Link(ZorkIds.NsPassage, Direction.South, ZorkIds.RoundRoom);
        Link(ZorkIds.NsPassage, Direction.NorthEast, ZorkIds.DeepCanyon);

        Link(ZorkIds.Chasm, Direction.Up, ZorkIds.EwPassage);
        Link(ZorkIds.Chasm, Direction.SouthWest, ZorkIds.EwPassage, false);
        Link(ZorkIds.Chasm, Direction.NorthEast, ZorkIds.ReservoirSouth);
        Link(ZorkIds.Chasm, Direction.South, ZorkIds.NsPassage);
        Block(ZorkIds.Chasm, Direction.Down, "Are you out of your mind?");

        Link(ZorkIds.DampCave, Direction.West, ZorkIds.LoudRoom);
        Link(ZorkIds.DampCave, Direction.East, ZorkIds.Beach2);
        Block(ZorkIds.DampCave, Direction.South, "It is too narrow for most insects.");

        Link(ZorkIds.TwistingPassage, Direction.East, ZorkIds.Cave2);
        Link(ZorkIds.TwistingPassage, Direction.North, ZorkIds.MirrorRoom1);

        Link(ZorkIds.WindingPassage, Direction.East, ZorkIds.Cave1);
        Link(ZorkIds.WindingPassage, Direction.North, ZorkIds.MirrorRoom2);

        Link(ZorkIds.NarrowPassage, Direction.North, ZorkIds.RoundRoom);
        Link(ZorkIds.NarrowPassage, Direction.South, ZorkIds.MirrorRoom2);

        Link(ZorkIds.ColdPassage, Direction.West, ZorkIds.SlideRoom);
        Link(ZorkIds.ColdPassage, Direction.South, ZorkIds.MirrorRoom1);

        // ENGINE-LIMIT: ZorkSimplifications.Mirrors — connections only; no mirror break or item swap.
        Link(ZorkIds.MirrorRoom1, Direction.East, ZorkIds.Cave2);
        Link(ZorkIds.MirrorRoom1, Direction.North, ZorkIds.ColdPassage);

        Link(ZorkIds.MirrorRoom2, Direction.West, ZorkIds.WindingPassage);
        Link(ZorkIds.MirrorRoom2, Direction.East, ZorkIds.Cave1);
        Link(ZorkIds.MirrorRoom2, Direction.North, ZorkIds.NarrowPassage);

        Link(ZorkIds.Cave1, Direction.West, ZorkIds.WindingPassage);
        Link(ZorkIds.Cave1, Direction.North, ZorkIds.MirrorRoom2);
        Link(ZorkIds.Cave1, Direction.Down, ZorkIds.EntranceToHades);

        Link(ZorkIds.Cave2, Direction.West, ZorkIds.TwistingPassage);
        Link(ZorkIds.Cave2, Direction.North, ZorkIds.MirrorRoom1);
        Link(ZorkIds.Cave2, Direction.Down, ZorkIds.AtlantisRoom);
        Link(ZorkIds.Cave2, Direction.South, ZorkIds.AtlantisRoom, false);

        Link(ZorkIds.Temple, Direction.North, ZorkIds.TorchRoom);
        Link(ZorkIds.Temple, Direction.South, ZorkIds.Altar);
        Link(ZorkIds.Temple, Direction.Down, ZorkIds.EgyptianRoom);
        Link(ZorkIds.Temple, Direction.East, ZorkIds.EgyptianRoom, false);
        Link(ZorkIds.Temple, Direction.Out, ZorkIds.TorchRoom, false);
        Link(ZorkIds.Temple, Direction.Up, ZorkIds.TorchRoom, false);

        Link(ZorkIds.Altar, Direction.North, ZorkIds.Temple);
        Link(ZorkIds.Altar, Direction.Down, ZorkIds.Cave1);

        Link(ZorkIds.TorchRoom, Direction.South, ZorkIds.Temple);
        Link(ZorkIds.TorchRoom, Direction.Down, ZorkIds.Temple, false);
        Block(ZorkIds.TorchRoom, Direction.Up, "You cannot reach the rope.");

        Link(ZorkIds.EgyptianRoom, Direction.West, ZorkIds.Temple);
        Link(ZorkIds.EgyptianRoom, Direction.Up, ZorkIds.Temple, false);

        Link(ZorkIds.EntranceToHades, Direction.Up, ZorkIds.Cave1);
        LinkOneWay(ZorkIds.EntranceToHades, Direction.In, ZorkIds.LandOfTheDead);
        LinkOneWay(ZorkIds.EntranceToHades, Direction.South, ZorkIds.LandOfTheDead);

        LinkOneWay(ZorkIds.LandOfTheDead, Direction.North, ZorkIds.EntranceToHades);
        LinkOneWay(ZorkIds.LandOfTheDead, Direction.Out, ZorkIds.EntranceToHades);
    }

    private void ConnectDamAndReservoir()
    {
        Link(ZorkIds.Dam, Direction.North, ZorkIds.DamLobby);
        Link(ZorkIds.Dam, Direction.South, ZorkIds.DeepCanyon);
        Link(ZorkIds.Dam, Direction.West, ZorkIds.ReservoirSouth);
        Link(ZorkIds.Dam, Direction.Down, ZorkIds.DamBase);
        Link(ZorkIds.Dam, Direction.East, ZorkIds.DamBase, false);

        Link(ZorkIds.DamLobby, Direction.South, ZorkIds.Dam);
        Link(ZorkIds.DamLobby, Direction.East, ZorkIds.MaintenanceRoom);
        Link(ZorkIds.DamLobby, Direction.North, ZorkIds.MaintenanceRoom, false);

        Link(ZorkIds.MaintenanceRoom, Direction.South, ZorkIds.DamLobby);
        Link(ZorkIds.MaintenanceRoom, Direction.West, ZorkIds.DamLobby, false);

        Link(ZorkIds.DamBase, Direction.Up, ZorkIds.Dam);
        Link(ZorkIds.DamBase, Direction.North, ZorkIds.Dam, false);

        Link(ZorkIds.ReservoirSouth, Direction.East, ZorkIds.Dam);
        Link(ZorkIds.ReservoirSouth, Direction.West, ZorkIds.StreamView);
        Link(ZorkIds.ReservoirSouth, Direction.SouthWest, ZorkIds.Chasm);
        Link(ZorkIds.ReservoirSouth, Direction.SouthEast, ZorkIds.DeepCanyon);

        Link(ZorkIds.Reservoir, Direction.North, ZorkIds.ReservoirNorth);
        Link(ZorkIds.Reservoir, Direction.South, ZorkIds.ReservoirSouth);
        Link(ZorkIds.Reservoir, Direction.West, ZorkIds.Stream);
        Link(ZorkIds.Reservoir, Direction.Up, ZorkIds.Stream, false);
        Block(ZorkIds.Reservoir, Direction.Down, "The dam blocks your way.");

        Link(ZorkIds.ReservoirNorth, Direction.South, ZorkIds.Reservoir);
        Link(ZorkIds.ReservoirNorth, Direction.North, ZorkIds.AtlantisRoom);
        Link(ZorkIds.ReservoirNorth, Direction.Up, ZorkIds.AtlantisRoom, false);

        Link(ZorkIds.AtlantisRoom, Direction.South, ZorkIds.ReservoirNorth);
        Link(ZorkIds.AtlantisRoom, Direction.Up, ZorkIds.Cave2);

        Link(ZorkIds.StreamView, Direction.East, ZorkIds.ReservoirSouth);
        Link(ZorkIds.Stream, Direction.Down, ZorkIds.Reservoir);
        Link(ZorkIds.Stream, Direction.East, ZorkIds.Reservoir, false);
        LinkOneWay(ZorkIds.Stream, Direction.Out, ZorkIds.StreamView);
    }

    // ENGINE-LIMIT: ZorkSimplifications.BoatAndRiver — river map exists; no board/disembark or boat-gated movement.
    private void ConnectRiver()
    {
        Link(ZorkIds.River1, Direction.East, ZorkIds.Shore);
        Block(ZorkIds.River1, Direction.Up, "You cannot go upstream due to strong currents.");

        Link(ZorkIds.FrigidRiver, Direction.Down, ZorkIds.River1);
        Link(ZorkIds.FrigidRiver, Direction.West, ZorkIds.Beach1);
        Link(ZorkIds.FrigidRiver, Direction.East, ZorkIds.SandyBeach);
        Block(ZorkIds.FrigidRiver, Direction.Up, "You cannot go upstream due to strong currents.");

        Link(ZorkIds.River2, Direction.Down, ZorkIds.FrigidRiver);
        Link(ZorkIds.River2, Direction.West, ZorkIds.Beach2);
        Block(ZorkIds.River2, Direction.Up, "You cannot go upstream due to strong currents.");

        Link(ZorkIds.River3, Direction.Down, ZorkIds.River2);
        Block(ZorkIds.River3, Direction.Up, "You cannot go upstream due to strong currents.");
        Block(ZorkIds.River3, Direction.West, "Just in time you steer away from the rocks.");
        Block(ZorkIds.River3, Direction.East, "The White Cliffs prevent your landing here.");

        Link(ZorkIds.River4, Direction.Down, ZorkIds.River3);
        Link(ZorkIds.River4, Direction.West, ZorkIds.DamBase);
        Block(ZorkIds.River4, Direction.Up, "You cannot go upstream due to strong currents.");
        Block(ZorkIds.River4, Direction.East, "The White Cliffs prevent your landing here.");

        Link(ZorkIds.Beach1, Direction.North, ZorkIds.Beach2);
        Link(ZorkIds.Beach2, Direction.South, ZorkIds.Beach1);
        Link(ZorkIds.Beach2, Direction.West, ZorkIds.DampCave);

        Link(ZorkIds.SandyBeach, Direction.NorthEast, ZorkIds.SandyCave);
        Link(ZorkIds.SandyBeach, Direction.South, ZorkIds.Shore);

        LinkOneWay(ZorkIds.SandyCave, Direction.SouthWest, ZorkIds.SandyBeach);

        Link(ZorkIds.Shore, Direction.North, ZorkIds.SandyBeach);
        Link(ZorkIds.Shore, Direction.South, ZorkIds.AragainFalls);

        Link(ZorkIds.AragainFalls, Direction.North, ZorkIds.Shore);
        LinkOneWay(ZorkIds.AragainFalls, Direction.Up, ZorkIds.OnTheRainbow);
        LinkOneWay(ZorkIds.AragainFalls, Direction.West, ZorkIds.OnTheRainbow);

        LinkOneWay(ZorkIds.OnTheRainbow, Direction.West, ZorkIds.EndOfRainbow);
        LinkOneWay(ZorkIds.OnTheRainbow, Direction.East, ZorkIds.AragainFalls);

        Link(ZorkIds.EndOfRainbow, Direction.SouthWest, ZorkIds.CanyonBottom);
    }

    private void ConnectCoalMine()
    {
        LinkOneWay(ZorkIds.SlideRoom, Direction.Down, ZorkIds.Cellar);
        Link(ZorkIds.SlideRoom, Direction.East, ZorkIds.ColdPassage);
        Link(ZorkIds.SlideRoom, Direction.North, ZorkIds.MineEntrance);

        Link(ZorkIds.MineEntrance, Direction.South, ZorkIds.SlideRoom);
        Link(ZorkIds.MineEntrance, Direction.West, ZorkIds.SqueakyRoom);
        LinkOneWay(ZorkIds.MineEntrance, Direction.In, ZorkIds.SqueakyRoom);

        Link(ZorkIds.SqueakyRoom, Direction.East, ZorkIds.MineEntrance);
        Link(ZorkIds.SqueakyRoom, Direction.North, ZorkIds.BatRoom);

        Link(ZorkIds.BatRoom, Direction.South, ZorkIds.SqueakyRoom);
        Link(ZorkIds.BatRoom, Direction.East, ZorkIds.ShaftRoom);

        LinkOneWay(ZorkIds.Mine1, Direction.Down, ZorkIds.LadderTop);
        LinkOneWay(ZorkIds.Mine1, Direction.West, ZorkIds.Mine1);
        LinkOneWay(ZorkIds.Mine1, Direction.North, ZorkIds.Mine2);

        LinkOneWay(ZorkIds.Mine2, Direction.SouthWest, ZorkIds.Mine1);
        LinkOneWay(ZorkIds.Mine2, Direction.South, ZorkIds.Mine2);
        LinkOneWay(ZorkIds.Mine2, Direction.East, ZorkIds.Mine3);

        LinkOneWay(ZorkIds.Mine3, Direction.SouthEast, ZorkIds.Mine2);
        LinkOneWay(ZorkIds.Mine3, Direction.South, ZorkIds.Mine4);
        LinkOneWay(ZorkIds.Mine3, Direction.North, ZorkIds.Mine3);

        LinkOneWay(ZorkIds.Mine4, Direction.NorthEast, ZorkIds.Mine3);
        LinkOneWay(ZorkIds.Mine4, Direction.East, ZorkIds.Mine4);
        LinkOneWay(ZorkIds.Mine4, Direction.North, ZorkIds.GasRoom);

        Link(ZorkIds.GasRoom, Direction.Up, ZorkIds.SmellyRoom);
        Link(ZorkIds.GasRoom, Direction.East, ZorkIds.Mine4);

        Link(ZorkIds.SmellyRoom, Direction.Down, ZorkIds.GasRoom);
        Link(ZorkIds.SmellyRoom, Direction.South, ZorkIds.ShaftRoom);

        Link(ZorkIds.ShaftRoom, Direction.West, ZorkIds.BatRoom);
        Link(ZorkIds.ShaftRoom, Direction.North, ZorkIds.SmellyRoom);
        Block(ZorkIds.ShaftRoom, Direction.Down, "You wouldn't fit and would die if you could.");

        Link(ZorkIds.LadderTop, Direction.Down, ZorkIds.LadderBottom);
        Link(ZorkIds.LadderTop, Direction.Up, ZorkIds.Mine1);

        Link(ZorkIds.LadderBottom, Direction.Up, ZorkIds.LadderTop);
        Link(ZorkIds.LadderBottom, Direction.South, ZorkIds.DeadEnd);
        Link(ZorkIds.LadderBottom, Direction.West, ZorkIds.TimberRoom);

        LinkOneWay(ZorkIds.DeadEnd, Direction.North, ZorkIds.LadderBottom);

        Link(ZorkIds.TimberRoom, Direction.East, ZorkIds.LadderBottom);
        Link(ZorkIds.TimberRoom, Direction.West, ZorkIds.DraftyRoom);

        Link(ZorkIds.DraftyRoom, Direction.South, ZorkIds.MachineRoom);
        Link(ZorkIds.DraftyRoom, Direction.East, ZorkIds.TimberRoom);
        LinkOneWay(ZorkIds.DraftyRoom, Direction.Out, ZorkIds.TimberRoom);

        Link(ZorkIds.MachineRoom, Direction.North, ZorkIds.DraftyRoom);
    }

    private void ConfigureConditionalExits()
    {
        // Kitchen window
        Exit toKitchen = EastOfHouse.Connect(Direction.In, Kitchen, reciprocal: false);
        toKitchen.Condition = ctx => ctx.Get(_windowOpen);
        toKitchen.BlockedMessage = "The window is too small to enter through.";
        Exit fromKitchen = Kitchen.Connect(Direction.Out, EastOfHouse, reciprocal: false);
        fromKitchen.Condition = ctx => ctx.Get(_windowOpen);
        fromKitchen.BlockedMessage = "The window is too small to exit through.";
        Exit toKitchenW = EastOfHouse.Connect(Direction.West, Kitchen, reciprocal: false);
        toKitchenW.Condition = ctx => ctx.Get(_windowOpen);
        toKitchenW.BlockedMessage = "The window is too small to enter through.";

        // Trap door
        Exit toCellar = LivingRoom.Connect(Direction.Down, Cellar, reciprocal: false);
        toCellar.Condition = ctx => ctx.Get(_trapDoorOpen);
        toCellar.BlockedMessage = "The trap door is closed.";
        Exit fromCellar = Cellar.Connect(Direction.Up, LivingRoom, reciprocal: false);
        fromCellar.Condition = ctx => ctx.Get(_trapDoorOpen);
        fromCellar.BlockedMessage = "The door at the top of the stairs is closed.";

        // Grating
        Exit toGrating = Clearing1.Connect(Direction.Down, GratingRoom, reciprocal: false);
        toGrating.Condition = ctx => ctx.Get(_gratingOpen);
        toGrating.BlockedMessage = "The grating is closed.";
        Exit fromGrating = GratingRoom.Connect(Direction.Up, Clearing1, reciprocal: false);
        fromGrating.Condition = ctx => ctx.Get(_gratingOpen);
        fromGrating.BlockedMessage = "The grating is closed.";

        // Troll
        ConfigureTrollExits();

        // Cyclops up
        R(ZorkIds.CyclopsRoom).ExitTo(Direction.Up)!.Condition = ctx =>
            ctx.Get(_cyclopsAsleep) || ctx.Get(_cyclopsDead);
        R(ZorkIds.CyclopsRoom).ExitTo(Direction.Up)!.BlockedMessage =
            "The cyclops doesn't look like he'll let you past.";

        // ENGINE-LIMIT: ZorkSimplifications.MagicPassage — magic flag from gallery visit, not gnome encounter.
        Exit toStrange = LivingRoom.Connect(Direction.West, StrangePassage, reciprocal: false);
        toStrange.Condition = ctx => ctx.Get(_magicFlag);
        toStrange.BlockedMessage = "The door is nailed shut.";

        // ENGINE-LIMIT: ZorkSimplifications.MagicPassage — _strangePassageOpen never set; cyclops wall break omitted.
        R(ZorkIds.CyclopsRoom).ExitTo(Direction.East)!.Condition = ctx => ctx.Get(_strangePassageOpen);
        R(ZorkIds.CyclopsRoom).ExitTo(Direction.East)!.BlockedMessage = "The east wall is solid rock.";

        // ENGINE-LIMIT: ZorkSimplifications.MagicPassage, Encumbrance — chimney flag on studio enter; no load check.
        Exit toStudio = Kitchen.Connect(Direction.Down, R(ZorkIds.Studio), reciprocal: false);
        toStudio.Condition = ctx => ctx.Get(_chimneyFlag);
        toStudio.BlockedMessage = "Only Santa Claus climbs down chimneys.";

        // Studio up to kitchen
        R(ZorkIds.Studio).Connect(Direction.Up, Kitchen, reciprocal: false).Condition = ctx => ctx.Get(_chimneyFlag);

        // Reservoir crossing
        ConfigureReservoirExits();

        // Rainbow
        ConfigureRainbowExits();

        // Hades
        R(ZorkIds.EntranceToHades).ExitTo(Direction.In)!.Condition = ctx => ctx.Get(_hadesOpen);
        R(ZorkIds.EntranceToHades).ExitTo(Direction.In)!.BlockedMessage =
            "Some invisible force prevents you from passing through the gate.";
        R(ZorkIds.EntranceToHades).ExitTo(Direction.South)!.Condition = ctx => ctx.Get(_hadesOpen);
        R(ZorkIds.EntranceToHades).ExitTo(Direction.South)!.BlockedMessage =
            "Some invisible force prevents you from passing through the gate.";

        // Dome rope
        R(ZorkIds.DomeRoom).Connect(Direction.Down, TorchRoom, reciprocal: false).Condition =
            ctx => ctx.Get(_domeRopeTied);
        R(ZorkIds.DomeRoom).ExitTo(Direction.Down)!.BlockedMessage =
            "You cannot reach the rope.";

        // Stone barrow after win
        Exit toBarrow = WestOfHouse.Connect(Direction.SouthWest, StoneBarrow, reciprocal: false);
        toBarrow.Condition = ctx => ctx.Get(_wonFlag);
        Exit toBarrowIn = WestOfHouse.Connect(Direction.In, StoneBarrow, reciprocal: false);
        toBarrowIn.Condition = ctx => ctx.Get(_wonFlag);

        // Beach boat restrictions handled in puzzles

        // ENGINE-LIMIT: ZorkSimplifications.GasRoom — gas explosion with open flame not implemented.
    }

    private void ConfigureTrollExits()
    {
        foreach (Direction dir in new[] { Direction.West, Direction.East })
        {
            if (TrollRoom.ExitTo(dir) is { } exit)
            {
                exit.Condition = ctx => ctx.Get(_trollDefeated) || ctx.Get(_trollKO) > 0;
                exit.BlockedMessage = "The troll fends you off with a menacing gesture.";
            }
        }
    }

    private void ConfigureReservoirExits()
    {
        foreach (Room room in new[] { R(ZorkIds.ReservoirSouth), R(ZorkIds.ReservoirNorth) })
        {
            if (room.ExitTo(Direction.North) is { } north && north.Destination == R(ZorkIds.Reservoir))
            {
                north.Condition = ctx => ctx.Get(_lowTide);
                north.BlockedMessage = "You would drown.";
            }
            if (room.ExitTo(Direction.South) is { } south && south.Destination == R(ZorkIds.Reservoir))
            {
                south.Condition = ctx => ctx.Get(_lowTide);
                south.BlockedMessage = "You would drown.";
            }
        }
    }

    private void ConfigureRainbowExits()
    {
        if (EndOfRainbow.ExitTo(Direction.East) is { } e) { e.Condition = ctx => ctx.Get(_rainbowSolid); e.BlockedMessage = "The rainbow is not solid enough to walk on."; }
        if (R(ZorkIds.AragainFalls).ExitTo(Direction.Up) is { } u) { u.Condition = ctx => ctx.Get(_rainbowSolid); u.BlockedMessage = "The rainbow is not solid enough to walk on."; }
        if (R(ZorkIds.AragainFalls).ExitTo(Direction.West) is { } w) { w.Condition = ctx => ctx.Get(_rainbowSolid); w.BlockedMessage = "The rainbow is not solid enough to walk on."; }
    }
}
