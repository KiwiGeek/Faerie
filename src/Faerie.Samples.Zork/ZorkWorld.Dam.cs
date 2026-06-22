using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

/// <summary>Flood Control Dam #3 — Infocom <c>1actions.zil</c> (historicalsource/zork1).</summary>
internal sealed partial class ZorkWorld
{
    private const int DamTimerTurns = 8;
    private const int MaintenanceFloodDeathLevel = 14;

    private static readonly string[] MaintenanceFloodLevels =
    [
        "up to your ankles.",
        "up to your shin.",
        "up to your knees.",
        "up to your hips.",
        "up to your waist.",
        "up to your chest.",
        "up to your neck.",
        "over your head.",
        "high in your lungs."
    ];

    internal Room DamLobby => R(ZorkIds.DamLobby);
    internal Room ReservoirSouth => R(ZorkIds.ReservoirSouth);
    internal Room ReservoirNorth => R(ZorkIds.ReservoirNorth);
    internal Room DeepCanyon => R(ZorkIds.DeepCanyon);
    internal Room DampCave => R(ZorkIds.DampCave);

    internal Thing BlueButton = null!;
    internal Thing YellowButton = null!;
    internal Thing BrownButton = null!;
    internal Thing RedButton = null!;
    internal Thing ControlPanel = null!;
    internal Thing Leak = null!;
    internal Thing Tube = null!;
    internal Thing Putty = null!;
    internal Thing ToolChest = null!;

    private StateKey<bool> _gateFlag = null!;
    private StateKey<bool> _gatesOpen = null!;
    private StateKey<int> _waterLevel = null!;
    private StateKey<bool> _maintenanceFlooded = null!;
    private StateKey<bool> _maintenanceLit = null!;
    private StateKey<bool> _damScoreAwarded = null!;

    private Verb _squeeze = null!;

    private void DefineDamThings()
    {
        BlueButton = Reg("blue_button", _b.Scenery("blue button").Called("button").Adjectives("blue")
            .Describe("A blue button."));
        YellowButton = Reg("yellow_button", _b.Scenery("yellow button").Called("button").Adjectives("yellow")
            .Describe("A yellow button."));
        BrownButton = Reg("brown_button", _b.Scenery("brown button").Called("button").Adjectives("brown")
            .Describe("A brown button."));
        RedButton = Reg("red_button", _b.Scenery("red button").Called("button").Adjectives("red")
            .Describe("A red button."));
        ControlPanel = Reg("control_panel", _b.Scenery("control panel").Called("panel").Adjectives("metal")
            .Describe("There is a control panel here, on which a large metal bolt is mounted."));
        Leak = Reg("leak", _b.Scenery("leak").Describe("A leak has occurred in a pipe on the east wall.").Concealed());
        Tube = Reg("tube", _b.Item("tube").Adjectives("small")
            .Describe("A small tube.").Container(open: false));
        Putty = Reg("putty", _b.Item("putty").Called("gunk").Adjectives("all-purpose")
            .Describe("A small quantity of all-purpose gunk."));
        ToolChest = Reg("tool_chest", _b.Scenery("tool chest").Called("chest").Adjectives("rusty")
            .Describe("The chests are all empty."));
    }

    private void PlaceDamThings()
    {
        BlueButton.StartsIn(MaintenanceRoom);
        YellowButton.StartsIn(MaintenanceRoom);
        BrownButton.StartsIn(MaintenanceRoom);
        RedButton.StartsIn(MaintenanceRoom);
        ControlPanel.StartsIn(Dam);
        Tube.StartsIn(MaintenanceRoom);
        Putty.StartsInside(Tube);
        ToolChest.StartsIn(MaintenanceRoom);
    }

    private void DefineDamState()
    {
        _gateFlag = _b.State("dam-gate-flag", false);
        _gatesOpen = _b.State("dam-gates-open", false);
        _waterLevel = _b.State("dam-water-level", 0);
        _maintenanceFlooded = _b.State("dam-maint-flooded", false);
        _maintenanceLit = _b.State("dam-maint-lit", false);
        _damScoreAwarded = _b.State("dam-score-awarded", false);
    }

    private void DefineDamAndReservoir()
    {
        DefineDamState();
        DefineDamThings();
        PlaceDamThings();

        _squeeze = _b.DefineVerb("squeeze", ["squeeze"], VerbForms.Transitive, SqueezeHandler);

        ConfigureDamRoomDescriptions();
        ConfigureMaintenanceAccess();

        WireDamButtons();
        WireDamBolt();
        WireDamLeakAndPutty();
        WireDamTimers();
        WireLoudRoomDamIntegration();
    }

    private bool LoudRoomQuiet(GameContext ctx) =>
        ctx.Get(_loudQuieted) || (!ctx.Get(_gatesOpen) && ctx.Get(_lowTide));

    private void WireDamButtons()
    {
        foreach (Thing button in new[] { BlueButton, YellowButton, BrownButton, RedButton })
        {
            _b.On(button).Before(_move, ctx => PushDamButton(ctx, button));
            _b.On(button).Before(_b.Verbs.Read!, ctx =>
            {
                ctx.Say("They're greek to you.");
                return VerbResult.Done;
            });
        }
    }

    private VerbResult PushDamButton(VerbContext ctx, Thing button)
    {
        if (!ctx.InRoom(MaintenanceRoom))
            return VerbResult.Pass;

        if (button == BlueButton)
        {
            if (ctx.Get(_waterLevel) != 0)
            {
                ctx.Say("The blue button appears to be jammed.");
                return VerbResult.Done;
            }
            Leak.Set(Attr.Concealed, false);
            ctx.Set(_waterLevel, 1);
            ctx.Say("There is a rumbling sound and a stream of water appears to burst from the east wall " +
                    "of the room (apparently, a leak has occurred in a pipe).");
            return VerbResult.Done;
        }

        if (button == RedButton)
        {
            bool lit = ctx.Get(_maintenanceLit);
            ctx.Set(_maintenanceLit, !lit);
            ctx.Say(lit
                ? "The lights within the room shut off."
                : "The lights within the room come on.");
            return VerbResult.Done;
        }

        if (button == BrownButton)
        {
            ctx.Set(_gateFlag, false);
            ctx.Say("Click.");
            return VerbResult.Done;
        }

        // Yellow button
        ctx.Set(_gateFlag, true);
        ctx.Say("Click.");
        return VerbResult.Done;
    }

    private void WireDamBolt()
    {
        _b.On(Bolt).Before(_turn, ctx =>
        {
            if (ctx.DirectObject != Bolt) return VerbResult.Pass;
            if (!ctx.Carrying(Wrench))
            {
                ctx.Say("You can't turn the bolt with your bare hands.");
                return VerbResult.Done;
            }
            if (!ctx.Get(_gateFlag))
            {
                ctx.Say("The bolt won't turn with your best effort.");
                return VerbResult.Done;
            }

            if (ctx.Get(_gatesOpen))
            {
                ctx.Set(_gatesOpen, false);
                ctx.Say("The sluice gates close and water starts to collect behind the dam.");
                ctx.CancelSchedule("dam-empty");
                ctx.ScheduleIn("dam-fill", DamTimerTurns, DamFillComplete);
            }
            else
            {
                ctx.Set(_gatesOpen, true);
                ctx.Say("The sluice gates open and water pours through the dam.");
                ctx.CancelSchedule("dam-fill");
                ctx.ScheduleIn("dam-empty", DamTimerTurns, DamDrainComplete);
            }
            return VerbResult.Done;
        });

        _b.On(Bolt).Before(_b.Verbs.Take!, ctx =>
        {
            ctx.Say("It is an integral part of the control panel.");
            return VerbResult.Done;
        });

        ControlPanel.Describe(DamControlPanelDescription);
    }

    private string DamControlPanelDescription(GameContext ctx)
    {
        string bubble = ctx.Get(_gateFlag)
            ? " Directly above the bolt is a small green plastic bubble which is glowing serenely."
            : " Directly above the bolt is a small green plastic bubble.";
        return "There is a control panel here, on which a large metal bolt is mounted." + bubble;
    }

    private void WireDamLeakAndPutty()
    {
        _b.On(Tube).Before(_squeeze, ctx =>
        {
            if (!ctx.Here(Tube))
                return VerbResult.Pass;
            if (!Tube.Has(Attr.Open))
            {
                ctx.Say("The tube is closed.");
                return VerbResult.Done;
            }
            if (!ctx.State.ContentsOf(Tube).Contains(Putty))
            {
                ctx.Say("The tube is apparently empty.");
                return VerbResult.Done;
            }
            ctx.Take(Putty);
            ctx.Say("The viscous material oozes into your hand.");
            return VerbResult.Done;
        });

        _b.On(Tube).Before(_b.Verbs.Open!, ctx =>
        {
            ctx.Say("The tube is closed.");
            return VerbResult.Pass;
        });

        _b.On(Leak).Before(_b.Verbs.Put!, ctx =>
        {
            if (ctx.DirectObject != Putty || ctx.IndirectObject != Leak)
                return VerbResult.Pass;
            FixMaintenanceLeak(ctx);
            return VerbResult.Done;
        });

        _b.On(ToolChest).Before(_b.Verbs.Take!, ctx =>
        {
            ctx.Say("The chests are so rusty and corroded that they crumble when you touch them.");
            ctx.Remove(ToolChest);
            return VerbResult.Done;
        });
        _b.On(ToolChest).Before(_b.Verbs.Open!, ctx =>
        {
            ctx.Say("The chests are already open.");
            return VerbResult.Done;
        });
    }

    private void FixMaintenanceLeak(GameContext ctx)
    {
        ctx.Remove(Putty);
        ctx.Set(_waterLevel, -1);
        ctx.Say("By some miracle of Zorkian technology, you have managed to stop the leak in the dam.");
    }

    private VerbResult SqueezeHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == Tube) return VerbResult.Pass;
        ctx.Say("You can't squeeze that.");
        return VerbResult.Done;
    }

    private void WireDamTimers()
    {
        _b.EveryTurn(MaintenanceFloodTick, when: ctx => ctx.Get(_waterLevel) > 0);
    }

    private void MaintenanceFloodTick(GameContext ctx)
    {
        if (ctx.Get(_waterLevel) <= 0) return;

        if (ctx.InRoom(MaintenanceRoom))
        {
            int index = Math.Clamp(ctx.Get(_waterLevel) - 1, 0, MaintenanceFloodLevels.Length - 1);
            ctx.Say($"The water level here is now {MaintenanceFloodLevels[index]}");
        }

        int next = ctx.Get(_waterLevel) + 1;
        ctx.Set(_waterLevel, next);

        if (next < MaintenanceFloodDeathLevel) return;

        ctx.Set(_maintenanceFlooded, true);
        if (ctx.InRoom(MaintenanceRoom))
            ctx.Die("I'm afraid you have done drowned yourself.");

        if (ctx.Get(_boatInflated) && (ctx.InRoom(MaintenanceRoom) || ctx.InRoom(Dam) || ctx.InRoom(DamLobby)))
            ctx.Die("The rising water carries the boat over the dam, down the river, and over the falls. Tsk, tsk.");
    }

    private void DamDrainComplete(GameContext ctx)
    {
        ctx.Set(_lowTide, true);
        TrunkOfJewels.Set(Attr.Concealed, false);
        if (!ctx.Get(_damScoreAwarded))
        {
            ctx.Set(_damScoreAwarded, true);
            ctx.AdjustScore(4);
        }

        if (ctx.InRoom(Reservoir))
            ctx.Say("You are on what used to be a large lake, but which is now a large mud pile. There are \"shores\" to the north and south.");
        else if (ctx.InRoom(DeepCanyon))
            ctx.Say("The roar of rushing water is quieter now.");
        else if (ctx.CurrentRoom == ReservoirSouth || ctx.CurrentRoom == ReservoirNorth)
            ctx.Say("The water level is now quite low here and you could easily cross over to the other side.");
    }

    private void DamFillComplete(GameContext ctx)
    {
        ctx.Set(_lowTide, false);
        TrunkOfJewels.Set(Attr.Concealed, true);

        if (ctx.InRoom(Reservoir) && !ctx.Carrying(Boat) && !ctx.State.Inventory.Contains(Boat))
        {
            ctx.Die("You are lifted up by the rising river! You try to swim, but the currents are too strong. " +
                    "You come closer, closer to the awesome structure of Flood Control Dam #3. The dam beckons to you. " +
                    "The roar of the water nearly deafens you, but you remain conscious as you tumble over the dam " +
                    "toward your certain doom among the rocks at its base.");
        }
        else if (ctx.InRoom(DeepCanyon))
            ctx.Say("A sound, like that of flowing water, starts to come from below.");
        else if (ctx.InRoom(LoudRoom))
        {
            ctx.Say("All of a sudden, an alarmingly loud roaring sound fills the room. Filled with fear, you scramble away.");
            ScrambleFromLoudRoom(ctx);
        }
        else if (ctx.CurrentRoom == ReservoirSouth || ctx.CurrentRoom == ReservoirNorth)
            ctx.Say("You notice that the water level has risen to the point that it is impossible to cross.");
    }

    private void WireLoudRoomDamIntegration()
    {
        LoudRoom.OnTurn = ctx =>
        {
            if (!ctx.InRoom(LoudRoom)) return;
            if (ctx.Get(_gatesOpen) && !ctx.Get(_lowTide))
                ScrambleFromLoudRoom(ctx);
        };
    }

    private void ScrambleFromLoudRoom(GameContext ctx)
    {
        Room[] escapes = [DampCave, RoundRoom, DeepCanyon];
        Room dest = escapes[ctx.Random.Next(escapes.Length)];
        ctx.Say("It is unbearably loud here, with an ear-splitting roar seeming to come from all around you. " +
                "There is a pounding in your head which won't stop. With a tremendous effort, you scramble out of the room.");
        ctx.MovePlayerTo(dest);
    }

    private void ConfigureMaintenanceAccess()
    {
        MaintenanceRoom.LitWhen(ctx => ctx.Get(_maintenanceLit));

        bool CanEnterMaintenance(GameContext ctx) => !ctx.Get(_maintenanceFlooded);

        foreach (Exit exit in DamLobby.Exits.Values.Where(e => e.Destination == MaintenanceRoom))
        {
            exit.Condition = CanEnterMaintenance;
            exit.BlockedMessage = "The room is full of water and cannot be entered.";
        }
        foreach (Exit exit in MaintenanceRoom.Exits.Values.Where(e => e.Destination == DamLobby))
        {
            exit.Condition = CanEnterMaintenance;
            exit.BlockedMessage = "The room is full of water and cannot be entered.";
        }
    }

    private void ConfigureDamRoomDescriptions()
    {
        Dam.Describe(DamLook);
        ReservoirSouth.Describe(ReservoirSouthLook);
        ReservoirNorth.Describe(ReservoirNorthLook);
        Reservoir.Describe(ReservoirLook);
        DeepCanyon.Describe(DeepCanyonLook);
        LoudRoom.Describe(LoudRoomLook);
    }

    private string DamLook(GameContext ctx)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("You are standing on the top of the Flood Control Dam #3, which was quite a tourist attraction in times far distant. ");
        sb.Append("There are paths to the north, south, and west, and a scramble down.");
        if (ctx.Get(_lowTide) && ctx.Get(_gatesOpen))
            sb.Append("\nThe water level behind the dam is low: The sluice gates have been opened. Water rushes through the dam and downstream.");
        else if (ctx.Get(_gatesOpen))
            sb.Append("\nThe sluice gates are open, and water rushes through the dam. The water level behind the dam is still high.");
        else if (ctx.Get(_lowTide))
            sb.Append("\nThe sluice gates are closed. The water level in the reservoir is quite low, but the level is rising quickly.");
        else
            sb.Append("\nThe sluice gates on the dam are closed. Behind the dam, there can be seen a wide reservoir. Water is pouring over the top of the now abandoned dam.");
        sb.Append('\n');
        sb.Append(DamControlPanelDescription(ctx));
        return sb.ToString();
    }

    private string ReservoirSouthLook(GameContext ctx)
    {
        string body = ctx.Get(_lowTide) && ctx.Get(_gatesOpen)
            ? "You are in a long room, to the north of which was formerly a lake. However, with the water level lowered, there is merely a wide stream running through the center of the room."
            : ctx.Get(_gatesOpen)
                ? "You are in a long room. To the north is a large lake, too deep to cross. You notice, however, that the water level appears to be dropping at a rapid rate. Before long, it might be possible to cross to the other side from here."
                : ctx.Get(_lowTide)
                    ? "You are in a long room, to the north of which is a wide area which was formerly a reservoir, but now is merely a stream. You notice, however, that the level of the stream is rising quickly and that before long it will be impossible to cross here."
                    : "You are in a long room on the south shore of a large lake, far too deep and wide for crossing.";
        return body + "\nThere is a path along the stream to the east or west, a steep pathway climbing southwest along the edge of a chasm, and a path leading into a canyon to the southeast.";
    }

    private string ReservoirNorthLook(GameContext ctx)
    {
        string body = ctx.Get(_lowTide) && ctx.Get(_gatesOpen)
            ? "You are in a large cavernous room, the south of which was formerly a lake. However, with the water level lowered, there is merely a wide stream running through there."
            : ctx.Get(_gatesOpen)
                ? "You are in a large cavernous area. To the south is a wide lake, whose water level appears to be falling rapidly."
                : ctx.Get(_lowTide)
                    ? "You are in a cavernous area, to the south of which is a very wide stream. The level of the stream is rising rapidly, and it appears that before long it will be impossible to cross to the other side."
                    : "You are in a large cavernous room, north of a large lake.";
        return body + "\nThere is a slimy stairway leaving the room to the north.";
    }

    private string ReservoirLook(GameContext ctx)
    {
        string body = ctx.Get(_lowTide)
            ? "You are on what used to be a large lake, but which is now a large mud pile. There are \"shores\" to the north and south."
            : "You are on the lake. Beaches can be seen north and south. Upstream a small stream enters the lake through a narrow cleft in the rocks. The dam can be seen downstream.";
        return body;
    }

    private string DeepCanyonLook(GameContext ctx)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("You are on the south edge of a deep canyon. Passages lead off to the east, northwest and southwest. A stairway leads down.");
        if (ctx.Get(_gatesOpen) && !ctx.Get(_lowTide))
            sb.Append(" You can hear a loud roaring sound, like that of rushing water, from below.");
        else if (!ctx.Get(_gatesOpen) && ctx.Get(_lowTide))
            { /* quiet — no extra line */ }
        else
            sb.Append(" You can hear the sound of flowing water from below.");
        return sb.ToString();
    }

    private string LoudRoomLook(GameContext ctx)
    {
        string intro = "This is a large room with a ceiling which cannot be detected from the ground. " +
                       "There is a narrow passage from east to west and a stone stairway leading upward.";
        return LoudRoomQuiet(ctx)
            ? intro + " The room is eerie in its quietness."
            : intro + " The room is deafeningly loud with an undetermined rushing sound. The sound seems to reverberate from all of the walls, making it difficult even to think.";
    }
}
