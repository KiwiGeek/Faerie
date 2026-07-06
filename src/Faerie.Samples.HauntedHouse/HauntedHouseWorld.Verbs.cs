using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;
using static Faerie.Verbs.StandardVerbIds;

namespace Faerie.Samples.HauntedHouse;

internal sealed partial class HauntedHouseWorld
{
    internal const string CannotDoThat = "Sorry - you cannot do that.";

    private static readonly string[] HintLines =
    [
        "You will need a light in dark places. A candle, matches and a candlestick may help.",
        "Deal with the bats using the aerosol spray.",
        "The vacuum cleaner needs batteries, then USE it when ghosts are about.",
        "Dig in the cellar with the barred window using the shovel.",
        "Swing the axe at the weak wall in the study.",
        "Unlock the front door with the key from the coat pocket.",
        "Read the magic spells, then SAY XZANFAR in the ice-cold chamber.",
        "Leave objects with LEAVE when you have collected too many to carry.",
    ];

    private static readonly string[] UsborneVerbList =
    [
        "HELP", "CARRYING", "GO", "N", "S", "W", "E", "U", "D", "GET", "TAKE", "OPEN", "EXAMINE", "READ", "SAY",
        "DIG", "SWING", "CLIMB", "LIGHT", "UNLIGHT", "SPRAY", "USE", "UNLOCK", "LEAVE", "SCORE", "SAVE", "LOAD", "QUIT", "HINT"
    ];

    private void DefineVerbs()
    {
        _house.DefineVerb(Help, ["help"], VerbForms.Intransitive, HelpHandler);
        _house.DefineVerb("carrying", ["carrying"], VerbForms.Intransitive, CarryingHandler);
        _house.DefineVerb(Take, ["get", "take"], VerbForms.Transitive | VerbForms.Intransitive, TakeHandler);
        _house.DefineVerb(Open, ["open"], VerbForms.Transitive, OpenHandler);
        _house.DefineVerb(Examine, ["examine"], VerbForms.Transitive | VerbForms.Intransitive, ExamineHandler);
        _house.DefineVerb(Read, ["read"], VerbForms.Transitive, ReadHandler);
        _house.DefineVerb("say", ["say"], VerbForms.Transitive, SayHandler);
        _house.DefineVerb("dig", ["dig"], VerbForms.Transitive | VerbForms.Intransitive, DigHandler);
        _house.DefineVerb("swing", ["swing"], VerbForms.Transitive, SwingHandler);
        _house.DefineVerb("climb", ["climb"], VerbForms.Transitive | VerbForms.Intransitive, ClimbHandler);
        _house.DefineVerb("light", ["light"], VerbForms.Transitive, LightHandler);
        _house.DefineVerb("off", ["off", "unlight"], VerbForms.Intransitive | VerbForms.Transitive, SnuffHandler);
        _house.DefineVerb("spray", ["spray"], VerbForms.Transitive, SprayHandler);
        _house.DefineVerb(Use, ["use"], VerbForms.Transitive, UseHandler);
        _house.DefineVerb(Unlock, ["unlock"], VerbForms.Transitive, UnlockHandler);
        _house.DefineVerb("leave", ["leave"], VerbForms.Transitive, LeaveHandler);
        _house.DefineVerb(Score, ["score"], VerbForms.Intransitive, ScoreHandler);
        _house.DefineVerb(Save, ["save"], VerbForms.Intransitive | VerbForms.Transitive, SaveHandler);
        _house.DefineVerb(Restore, ["load", "restore"], VerbForms.Intransitive | VerbForms.Transitive, RestoreHandler);
        _house.DefineVerb(Quit, ["quit"], VerbForms.Intransitive, QuitHandler);
        _house.DefineVerb("hint", ["hint"], VerbForms.Intransitive, HintHandler);
    }

    private VerbResult HelpHandler(VerbContext ctx)
    {
        ctx.Say("Words I know:");
        ctx.Say(string.Join(", ", UsborneVerbList));
        return VerbResult.Done;
    }

    private VerbResult CarryingHandler(VerbContext ctx)
    {
        List<Thing> carried = ctx.State.Inventory.ToList();
        ctx.Say("You are carrying:");
        if (carried.Count == 0)
            ctx.Say("");
        else
            ctx.Say(string.Join(", ", carried.Select(t => VerbText.A(t))));
        return VerbResult.Done;
    }

    private VerbResult TakeHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing)
        {
            ctx.Say("?");
            return VerbResult.Done;
        }

        if (!IsTreasure(thing))
        {
            ctx.Say($"You cannot get {ctx.DirectObjectText ?? thing.Name}");
            return VerbResult.Done;
        }

        if (!IsHere(ctx, thing))
        {
            ctx.Say("It is not here");
            return VerbResult.Done;
        }

        if (ctx.Carrying(thing))
        {
            ctx.Say("You already have it");
            return VerbResult.Done;
        }

        if (!Encumbrance.CanTake(ctx, thing))
        {
            ctx.Say(Encumbrance.TakeBlockedMessage(ctx, thing));
            return VerbResult.Done;
        }

        ctx.Take(thing);
        ctx.Say("You now have it.");
        return VerbResult.Done;
    }

    private VerbResult OpenHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing)
            return Fail(ctx);

        if (ctx.CurrentRoom == Study && (thing == Drawer || thing == Desk))
        {
            ctx.Set(DrawerClosed, false);
            Candle.Concealed(false);
            ctx.Say("The drawer is now open");
            return VerbResult.Done;
        }

        if (ctx.CurrentRoom == LockedDoorHall && thing == FrontDoor)
        {
            ctx.Say("It is locked");
            return VerbResult.Done;
        }

        if (ctx.CurrentRoom == DeepCellar && thing == Coffin)
        {
            ctx.Set(CoffinClosed, false);
            Ring.Concealed(false);
            ctx.Say("It is now open");
            return VerbResult.Done;
        }

        return Fail(ctx);
    }

    private VerbResult ExamineHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing)
            return Fail(ctx);

        if (thing == VacuumCleaner)
        {
            ctx.Say("It needs batteries");
            return VerbResult.Done;
        }

        if (thing == Coat && ctx.Get(CoatUnsearched))
        {
            ctx.Set(CoatUnsearched, false);
            Key.Concealed(false);
            ctx.Say("Something falls out of the pocket.");
            return VerbResult.Done;
        }

        if (thing == Rubbish)
        {
            ctx.Say("That's disgusting!");
            return VerbResult.Done;
        }

        if (thing == Drawer || thing == Desk)
        {
            ctx.Say("There is a drawer");
            return VerbResult.Done;
        }

        if (thing == Books || thing == MagicSpells)
        {
            ctx.DirectObject = thing;
            return ReadHandler(ctx);
        }

        if (ctx.CurrentRoom == Study && thing == WeakWall)
        {
            ctx.Say("There is something beyond.....");
            return VerbResult.Done;
        }

        if (thing == Coffin)
        {
            ctx.DirectObject = thing;
            return OpenHandler(ctx);
        }

        return Fail(ctx);
    }

    private VerbResult ReadHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing)
            return Fail(ctx);

        if (ctx.CurrentRoom == Library && thing == Books)
        {
            ctx.Say("They are demonic works");
            return VerbResult.Done;
        }

        if ((thing == MagicSpells || thing == Xzanfar) && ctx.Carrying(MagicSpells) && !ctx.Get(SpellsBarrierDown))
        {
            ctx.Say("It says:'Use this word with care _ XZANFAR'");
            return VerbResult.Done;
        }

        if (ctx.Carrying(OldScroll) && thing == OldScroll)
        {
            ctx.Say("The writing is in a strange language");
            return VerbResult.Done;
        }

        return Fail(ctx);
    }

    private VerbResult SayHandler(VerbContext ctx)
    {
        string word = ctx.DirectObjectText ?? ctx.DirectObject?.Name ?? "";
        ctx.Say($"Ready '{word}'");

        if (!ctx.Carrying(MagicSpells) || ctx.DirectObject != Xzanfar)
            return VerbResult.Done;

        if (ctx.CurrentRoom != IceColdChamber)
        {
            Room[] rooms = _house.World.Rooms.ToArray();
            ctx.MovePlayerTo(rooms[ctx.Random.Next(rooms.Length)]);
            ctx.Say("You suddenly feel very faint and have to close your eyes. When you open them you realize that something magical has happened.");
            return VerbResult.Done;
        }

        ctx.Set(SpellsBarrierDown, true);
        return VerbResult.Done;
    }

    private VerbResult DigHandler(VerbContext ctx)
    {
        if (!ctx.Carrying(Shovel))
            return Fail(ctx);

        ctx.Say("You made a lovely little hole!");

        if (ctx.CurrentRoom != BarredCellar || ctx.Get(BarsDugOut))
            return VerbResult.Done;

        ctx.Set(BarsDugOut, true);
        BarredCellar.Name = "In a cellar with a barred window. There is a hole in the wall";
        BarredCellar.East(CliffPath1, reciprocal: false);
        ctx.Say("You have dug the bars out");
        return VerbResult.Done;
    }

    private VerbResult SwingHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null)
            return Fail(ctx);

        if (ctx.CurrentRoom == BlastedTree && !ctx.Carrying(Rope))
        {
            ctx.Say("This is no time to play games!");
            return VerbResult.Done;
        }

        if (ctx.DirectObject == Rope && ctx.Carrying(Rope))
        {
            ctx.Say("You swung it");
            return VerbResult.Done;
        }

        if (ctx.DirectObject == Axe && ctx.Carrying(Axe))
        {
            ctx.Say("Whoooosshhh!");
            if (ctx.CurrentRoom == Study && ctx.Get(WeakWallIntact))
            {
                ctx.Set(WeakWallIntact, false);
                Study.Name = "In a study with a secret room connected";
                Study.SetExit(Direction.North, SecretRoom);
                ctx.Say("You have broken the thin wall");
            }
            return VerbResult.Done;
        }

        return Fail(ctx);
    }

    private VerbResult ClimbHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == Rope && ctx.Carrying(Rope))
        {
            ctx.Say("It is not attached to anything!");
            return VerbResult.Done;
        }

        if (ctx.CurrentRoom != BlastedTree)
            return Fail(ctx);

        if (!ctx.Get(TreeClimbReady))
        {
            ctx.Set(TreeClimbReady, true);
            ctx.Say("You see thick forest and a cliff to the south");
            return VerbResult.Done;
        }

        ctx.Say("Going down!");
        ctx.Set(TreeClimbing, true);
        ctx.Set(TreeClimbReady, false);
        return VerbResult.Done;
    }

    private VerbResult LightHandler(VerbContext ctx)
    {
        if (ctx.DirectObject != Candle || !ctx.Carrying(Candle))
            return Fail(ctx);

        if (!ctx.Carrying(Candlestick))
        {
            ctx.Say("It will burn your hands!");
            return VerbResult.Done;
        }

        if (!ctx.Carrying(Matches))
        {
            ctx.Say("You have nothing to light it with!");
            return VerbResult.Done;
        }

        ctx.Set(CandleLit, true);
        Candle.Set(Attr.Lit);
        Candle.Set(Attr.LightSource);
        ctx.Say("It casts a flickering light");
        return VerbResult.Done;
    }

    private VerbResult SnuffHandler(VerbContext ctx)
    {
        if (!ctx.Get(CandleLit))
            return Fail(ctx);

        ctx.Set(CandleLit, false);
        Candle.Set(Attr.Lit, false);
        ctx.Say("Your candle is out");
        return VerbResult.Done;
    }

    private VerbResult SprayHandler(VerbContext ctx)
    {
        if (ctx.DirectObject != Bats || !ctx.Carrying(AerosolSpray))
            return Fail(ctx);

        ctx.Say("Pfffft! Got them!");
        ctx.Set(BatsPresent, false);
        return VerbResult.Done;
    }

    private VerbResult UseHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == VacuumCleaner && ctx.Carrying(VacuumCleaner) && ctx.Carrying(Batteries))
        {
            bool wasOn = ctx.Get(VacuumOn);
            ctx.Set(VacuumOn, true);

            if (ctx.Get(GhostsBlocking))
            {
                ctx.Set(GhostsBlocking, false);
                ctx.Say("Whizzzz! You have vacuumed the ghosts up!");
                return VerbResult.Done;
            }

            if (!wasOn)
                ctx.Say("It is switched on");
            return VerbResult.Done;
        }

        return Fail(ctx);
    }

    private VerbResult UnlockHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing)
            return Fail(ctx);

        if (ctx.CurrentRoom == Study && (thing == Drawer || thing == Desk))
            return OpenHandler(ctx);

        if (ctx.CurrentRoom == LockedDoorHall && thing == FrontDoor &&
            ctx.Get(FrontDoorLocked) && ctx.Carrying(Key))
        {
            ctx.Set(FrontDoorLocked, false);
            LockedDoorHall.Name = "By a huge open door";
            LockedDoorHall.South(MarbleStairs, reciprocal: false);
            ctx.Say("The key turns!");
            return VerbResult.Done;
        }

        return Fail(ctx);
    }

    private VerbResult LeaveHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing || !ctx.Carrying(thing))
            return Fail(ctx);

        ctx.PlaceHere(thing);
        ctx.Say("Done");
        return VerbResult.Done;
    }

    private VerbResult ScoreHandler(VerbContext ctx)
    {
        int score = CarriedTreasureCount(ctx);

        // The walkthrough reaches "you have everything" with 16 objects carried (every treasure
        // except the rope - which is used in place at the blasted tree and never picked up - and
        // the small boat, which must be left behind at the marsh). jbanes/haunted's C reimplementation
        // uses a literal constant of 17 here, but that never actually triggers given the rope is
        // not obtainable in normal play; 16 is the value validated against the full walkthrough.
        const int allTreasuresExceptBoat = 16;

        if (score == allTreasuresExceptBoat && !ctx.Carrying(SmallBoat) && ctx.CurrentRoom != IronGate)
        {
            ctx.Say("You have everything");
            ctx.Say("Return to the gate for your final score:");
            return VerbResult.Done;
        }

        if (score == allTreasuresExceptBoat && ctx.CurrentRoom == IronGate && !ctx.Carrying(SmallBoat))
        {
            int doubled = score * 2;
            ctx.Say("DOUBLE SCORE FOR REACHING HERE!");
            ctx.Say($"Your score is {doubled}");
            ctx.Win("Well done! You have finished the game");
            return VerbResult.Done;
        }

        ctx.Say($"Your score is {score}");
        if (score > 18)
            ctx.Win("Well done! You have finished the game");
        return VerbResult.Done;
    }

    private static VerbResult SaveHandler(VerbContext ctx)
    {
        ctx.Engine.RequestSave(ctx.DirectObjectText);
        return VerbResult.Done;
    }

    private static VerbResult RestoreHandler(VerbContext ctx)
    {
        ctx.Engine.RequestRestore(ctx.DirectObjectText);
        return VerbResult.Done;
    }

    private VerbResult QuitHandler(VerbContext ctx)
    {
        char yn = ctx.PromptKey("ARE YOU SURE YOU WANT TO QUIT ", "YN");
        if (yn is 'N' or 'n')
        {
            ctx.Say("GET ON WITH THE ADVENTURE!");
            return VerbResult.Done;
        }

        string save = ctx.PromptLine("IF YOU WANT TO SAVE YOUR POSITION, TYPE 'SAVE'").Trim();
        if (save.Equals("save", StringComparison.OrdinalIgnoreCase))
            ctx.Engine.RequestSave();
        else
            ctx.Engine.RequestQuit();
        return VerbResult.Done;
    }

    private VerbResult HintHandler(VerbContext ctx)
    {
        int i = ctx.Get(HintIndex);
        ctx.Say(HintLines[i % HintLines.Length]);
        ctx.Set(HintIndex, i + 1);
        return VerbResult.Done;
    }

    private static VerbResult Fail(VerbContext ctx)
    {
        ctx.Say(CannotDoThat);
        return VerbResult.Done;
    }
}
