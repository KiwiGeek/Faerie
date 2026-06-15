using Nixie.Building;
using Nixie.Model;
using Nixie.Presentation;
using Nixie.Runtime;
using Nixie.Verbs;

namespace Nixie.Sample.HauntedHouse;

/// <summary>
/// A complete sample adventure built entirely on the fluent engine. It is an original retelling in
/// the spirit of the old Usborne "Haunted House" type-in: explore a derelict mansion, find a light,
/// keep a crucifix between you and what lives in the cellar, recover the idol, and escape through
/// the gate. Every room, item and creature below is wired together by C# reference -- no strings.
/// </summary>
public static class HauntedHouseGame
{
    public static Game Build()
    {
        GameBuilder b = GameBuilder.Create("The Haunted House")
            .By("A sample for the Text Adventure engine")
            .WithMaxScore(100)
            .WithWindowTitle("The Haunted House — a Text Adventure")
            // .WithWindowIcon("avares://HauntedHouse/Assets/icon.ico")
            .WithFont("avares://HauntedHouse/Assets/Fonts")   // the game embeds its own VGA font
            .WithCursor(TerminalCursor.Block)
            .AddStandardVerbs();

        // Puzzle flags (global, so they survive save/restore).
        StateKey<bool> idolScored = b.State("idol-scored", false);

        // ---- rooms ------------------------------------------------------------------------

        Room road = b.Room("Country Road")
            .Describe("A quiet country road winds away beneath the moon. You are free of the house at last.");

        Room frontGate = b.Room("Front Gate")
            .Describe("You stand at a rusted iron gate. Beyond it, to the south, a road leads away to safety. " +
                      "An overgrown drive climbs north toward a black shape of gables and chimneys.");

        Room drive = b.Room("Overgrown Drive")
            .Describe("Weeds split the gravel of a long drive. The gate lies south; the porch of the house waits north.");

        Room porch = b.Room("Front Porch")
            .Describe("A sagging porch shelters a heavy front door to the north. A cracked plant pot squats by the step. " +
                      "The drive runs back south.");

        Room hall = b.Room("Entrance Hall")
            .Describe("Dust hangs in the air of a grand, ruined hall. Doorways lead off in every direction, and a wide " +
                      "staircase climbs into the dark above. The front door is to the south.");

        Room dining = b.Room("Dining Room")
            .Describe("A long table is set for a dinner that never ended: rotted food, toppled chairs, a film of grey dust. " +
                      "The hall is south, the kitchen west.");

        Room kitchen = b.Room("Kitchen")
            .Describe("A cold flagstone kitchen. A great iron range has long gone out. A narrow stair drops down into the " +
                      "cellar, the pantry lies north, and the dining room is east.");

        Room pantry = b.Room("Pantry")
            .Describe("Empty shelves and the smell of mice. A little daylight would help in here.")
            .Dark();

        Room sitting = b.Room("Sitting Room")
            .Describe("Sheeted furniture huddles like sleeping animals. A cold hearth faces a cracked mirror. The hall is " +
                      "east; a study opens north.");

        Room study = b.Room("Study")
            .Describe("A scholar's study gone to ruin. A great desk dominates the room, its papers spilled across the floor. " +
                      "The sitting room is south.");

        Room library = b.Room("Library")
            .Describe("Shelves of swollen, mildewed books rise to the ceiling. A reading desk stands in the centre. The hall " +
                      "lies west.");

        Room cellar = b.Room("Cellar")
            .Describe("A vaulted brick cellar, more crypt than store-room. Something has been sleeping down here. A battered " +
                      "trunk sits against the far wall. The stair climbs back up.")
            .Dark();

        Room landing = b.Room("Landing")
            .Describe("A railed landing overlooks the hall below. Bedroom doors lead off, and a hatch in the ceiling opens " +
                      "into the attic.");

        Room bedroom = b.Room("Master Bedroom")
            .Describe("A four-poster bed, its drapes long rotted, fills the master bedroom. The landing is south.");

        Room bathroom = b.Room("Bathroom")
            .Describe("Black mould blooms across cracked tiles. A claw-footed bath holds an inch of brackish water. The " +
                      "landing is west.");

        Room attic = b.Room("Attic")
            .Describe("The attic smells of bird-droppings and old timber. On a wooden crate, something gleams. A hatch " +
                      "leads back down.")
            .Dark();

        // ---- exits ------------------------------------------------------------------------

        frontGate.North(drive);
        drive.North(porch);
        porch.North(hall);          // gated by the front door (set below)
        hall.North(dining);
        hall.East(library);
        hall.West(sitting);
        hall.Up(landing);
        dining.West(kitchen);
        kitchen.North(pantry);
        kitchen.Down(cellar);
        sitting.North(study);
        landing.North(bedroom);
        landing.East(bathroom);
        landing.Up(attic);

        // The road out is south of the front gate, behind the locked iron gate.
        Exit toRoad = frontGate.Connect(Direction.South, road, reciprocal: false);

        // ---- items & scenery --------------------------------------------------------------

        Thing brassKey = b.Item("brass key").Adjectives("brass", "small")
            .Describe("A small brass key, green with age.");

        Thing rustyKey = b.Item("rusty key").Adjectives("rusty", "iron", "big")
            .Describe("A big iron key, scabbed with rust. It looks made for a gate.");

        Thing candle = b.Item("candle").Adjectives("wax", "white")
            .Describe("A stub of white candle. Unlit, it is no use in the dark.")
            .LightSource(lit: false);

        Thing matches = b.Item("matches").Plural().Adjectives("box", "match")
            .Called("matchbox", "box of matches")
            .Describe("A battered box with a few dry matches left inside.");

        Thing crucifix = b.Item("silver crucifix").Adjectives("silver", "holy")
            .Called("cross", "crucifix")
            .Describe("A heavy silver crucifix on a chain. It feels reassuringly solid.")
            .Wearable();

        Thing idol = b.Item("golden idol").Adjectives("golden", "gold")
            .Called("idol", "statue", "treasure")
            .Describe("A small, astonishingly heavy idol of solid gold. This is treasure worth braving a haunting for.")
            .InitialText("On the crate, a {fg:gold}golden idol{/} gleams in your light.");

        Thing book = b.Item("old book").Adjectives("old", "leather", "diary")
            .Called("book", "diary", "journal")
            .Describe("A water-stained diary in a cracked leather cover.")
            .Readable("The last entry is shaky: \"The cellar is no longer ours after dark. I keep the silver cross " +
                      "always about my neck now. The idol in the attic is cursed, they say... but a man could buy a new " +
                      "life with it, if he could only reach the gate.\"");

        // Scenery / containers.
        Thing pot = b.Scenery("plant pot").Adjectives("cracked", "flower", "clay")
            .Called("pot", "plantpot")
            .Describe("A cracked clay pot holding nothing but dead soil. Something might be buried in it.")
            .Container(open: true);

        Thing drawer = b.Scenery("kitchen drawer").Adjectives("kitchen")
            .Called("drawer")
            .Describe("A swollen wooden drawer in the kitchen table.")
            .Container(open: false);

        Thing desk = b.Scenery("reading desk").Adjectives("reading", "writing")
            .Called("desk")
            .Describe("A sturdy reading desk, its surface scarred with candle burns.")
            .Supporter();

        Thing trunk = b.Scenery("trunk").Adjectives("battered", "old", "wooden")
            .Called("chest", "trunk")
            .Describe("A battered wooden trunk, unlocked but firmly shut.")
            .Container(open: false);

        Thing frontDoor = b.Scenery("front door").Adjectives("front", "heavy", "oak")
            .Called("door")
            .Describe("A heavy oak door, weathered grey. It is the way into the house.")
            .LockedWith(brassKey);

        Thing ironGate = b.Scenery("iron gate").Adjectives("iron", "rusty", "rusted")
            .Called("gate")
            .Describe("A tall iron gate between you and the road. Its lock is crusted with rust.")
            .LockedWith(rustyKey);

        // ---- creatures --------------------------------------------------------------------

        Thing vampire = b.Creature("vampire").Adjectives("pale", "sleeping")
            .Called("creature", "thing", "count")
            .Describe("A gaunt, pale figure that watches you with a terrible patience.")
            .InitialText("{fg:bloodred}A vampire rises from the shadows!{/}");

        Thing ghost = b.Creature("ghost").Adjectives("grey", "pale")
            .Called("spectre", "phantom", "spirit")
            .Describe("A translucent grey figure drifts an inch above the floor, mouthing words you cannot hear.")
            .Concealed();

        // ---- initial placement ------------------------------------------------------------

        brassKey.StartsInside(pot).Concealed();   // found by searching the pot
        pot.StartsIn(porch);
        frontDoor.StartsIn(porch);
        ironGate.StartsIn(frontGate);
        drawer.StartsIn(kitchen);
        matches.StartsInside(drawer);
        candle.StartsOn(desk).InitialText("");     // listed via the desk
        desk.StartsIn(study);
        book.StartsIn(library);
        crucifix.StartsIn(library).InitialText("A {fg:lightcyan}silver crucifix{/} hangs on the wall here.");
        trunk.StartsIn(cellar);
        rustyKey.StartsInside(trunk);
        idol.StartsIn(attic);
        vampire.StartsIn(cellar);
        ghost.StartsIn(landing);

        // ---- doors gate their exits -------------------------------------------------------

        AttachDoor(porch, Direction.North, frontDoor);
        toRoad.Door = ironGate;

        // ---- reactions & puzzles ----------------------------------------------------------

        // The candle only lights if you are holding matches.
        b.On(candle).Before(b.Verbs.SwitchOn!, ctx =>
        {
            if (!ctx.Carrying(matches))
            {
                ctx.Say("You have nothing to light it with.");
                return VerbResult.Done;
            }
            return VerbResult.Pass;
        });

        // Reading the diary, or examining the desk, nudges the player.
        b.On(idol).After(b.Verbs.Take!, ctx =>
        {
            if (ctx.Carrying(idol) && !ctx.Get(idolScored))
            {
                ctx.Set(idolScored, true);
                ctx.State.Score += 50;
                ctx.Say("{fg:gold}You have the idol! (+50){/}");
            }
            return VerbResult.Pass;
        });

        // The cellar belongs to the vampire after dark unless you carry the crucifix.
        cellar.OnEnter = ctx =>
        {
            bool protectedByCross = ctx.Carrying(crucifix) || ctx.Wearing(crucifix);
            if (!protectedByCross)
                ctx.Lose("{fg:bloodred}The vampire is upon you before you can cry out. The last thing you feel is " +
                         "its cold hands at your throat.{/}");
            else
                ctx.Say("{fg:lightcyan}The vampire hisses and recoils from the silver crucifix, pressing back into its " +
                        "corner. It will not touch you while you hold the cross.{/}");
        };

        // A roaming ghost haunts the upstairs for atmosphere (harmless).
        Room[] upstairs = [landing, bedroom, bathroom, attic];
        b.EveryTurn(ctx =>
        {
            if (upstairs.Contains(ctx.CurrentRoom) && ctx.Random.Next(3) == 0)
                ctx.Say("{fg:lightcyan}{blink}A cold draught raises the hair on your neck.{/}{/}");
        });

        // Escaping: stepping onto the road ends the game.
        road.OnEnter = ctx =>
        {
            if (ctx.Carrying(idol))
            {
                ctx.State.Score += 50;
                ctx.Win("{fg:lightgreen}You slip through the gate and onto the road, the golden idol heavy in your pocket. " +
                        "You are rich, you are free, and you will never come back. (+50){/}");
            }
            else
            {
                ctx.Win("You stumble out onto the road and gulp the clean night air. You escaped with your life -- " +
                        "though you wonder what treasure you left behind in the dark.");
            }
        };

        // ---- presentation -----------------------------------------------------------------

        b.WithDefaultTitleBar();
        b.WithStatusBar(ctx =>
        {
            bool carryingLight = ctx.State.Inventory.Concat(ctx.State.Worn)
                .Any(t => t.Has(Attr.LightSource) && t.Has(Attr.Lit));
            string light = carryingLight ? "{fg:yellow}lit{/}" : "{fg:darkgray}no light{/}";
            return new BarContent
            {
                Left = $" {ctx.CurrentRoom.Name}",
                Right = $"{light}  F11 fullscreen ",
                Style = new TextStyle(TerminalColor.White, TerminalColor.Blue)
            };
        });

        b.WithIntro(
            "{fg:white}{bold}THE HAUNTED HOUSE{/}{/}\n" +
            "{fg:darkgray}an original sample for the Text Adventure engine{/}\n\n" +
            "Night has caught you at the gates of the old Mordent house. They say no one who enters after dark " +
            "comes out the same way they went in -- if they come out at all. But they also say there is gold inside. " +
            "\n\nType {fg:yellow}HELP{/} at any time for commands.");

        b.OnStart(ctx => ctx.Get(idolScored)); // register the flag so saves capture it
        b.StartIn(frontGate);

        return b.Build();
    }

    /// <summary>Sets a door on an exit and on its reciprocal so it gates travel in both directions.</summary>
    private