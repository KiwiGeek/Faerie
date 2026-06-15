using Nixie.Model;
using Nixie.Runtime;
using Nixie.Verbs;

namespace TextAdventure.Sample.Zork;

/// <summary>Puzzle wiring for the Zork sample. Each simplified behavior references <see cref="ZorkSimplifications"/>.</summary>
internal sealed partial class ZorkWorld
{
    private Verb _go = null!;
    private Verb _attack = null!;
    private Verb _move = null!;
    private Verb _climb = null!;
    private Verb _inflate = null!;
    private Verb _deflate = null!;
    private Verb _echo = null!;
    private Verb _yell = null!;
    private Verb _blast = null!;
    private Verb _turn = null!;
    private Verb _wind = null!;
    private Verb _dig = null!;
    private Verb _pray = null!;
    private Verb _wave = null!;
    private Verb _tie = null!;
    private Verb _temple = null!;
    private Verb _ring = null!;

    // ENGINE-LIMIT: ZorkSimplifications.Verbs — only a minimal custom verb set; many Infocom verbs absent.
    // Go omits "move" so the object-move verb can handle "move rug" (AddStandardVerbs binds move to Go).
    private void DefineCustomVerbs()
    {
        _go = _b.DefineVerb(StandardVerbIds.Go, ["go", "walk", "run", "head"],
            VerbForms.Transitive | VerbForms.Intransitive, GoHandler);

        _attack = _b.DefineVerb("attack", ["attack", "kill", "fight", "hit", "strike", "swing"],
            VerbForms.Transitive | VerbForms.Ditransitive, AttackHandler);
        _move = _b.DefineVerb("move", ["move", "push", "pull", "slide"], VerbForms.Transitive, MoveHandler);
        _climb = _b.DefineVerb("climb", ["climb", "scale"], VerbForms.Transitive | VerbForms.Intransitive, ClimbHandler);
        _inflate = _b.DefineVerb("inflate", ["inflate", "blow up", "pump up"],
            VerbForms.Transitive | VerbForms.Ditransitive, InflateHandler);
        _deflate = _b.DefineVerb("deflate", ["deflate"], VerbForms.Transitive, DeflateHandler);
        _echo = _b.DefineVerb("echo", ["echo"], VerbForms.Transitive, EchoHandler);
        _yell = _b.DefineVerb("yell", ["yell", "scream", "shout"], VerbForms.Transitive | VerbForms.Intransitive, YellHandler);
        _blast = _b.DefineVerb("blast", ["blast"], VerbForms.Transitive, BlastHandler);
        _turn = _b.DefineVerb("turn", ["turn", "twist"], VerbForms.Transitive, TurnHandler);
        _wind = _b.DefineVerb("wind", ["wind"], VerbForms.Transitive, WindHandler);
        _dig = _b.DefineVerb("dig", ["dig"], VerbForms.Transitive | VerbForms.Ditransitive, DigHandler);
        _pray = _b.DefineVerb("pray", ["pray"], VerbForms.Intransitive, PrayHandler);
        _wave = _b.DefineVerb("wave", ["wave"], VerbForms.Transitive, WaveHandler);
        _tie = _b.DefineVerb("tie", ["tie"], VerbForms.Transitive | VerbForms.Ditransitive, TieHandler);
        _temple = _b.DefineVerb("temple", ["temple"], VerbForms.Intransitive, TempleHandler);
        _ring = _b.DefineVerb("ring", ["ring"], VerbForms.Transitive, RingHandler);
    }

    private void DefinePuzzles()
    {
        DefineMailboxAndLeaflet();
        DefineHouseEntry();
        DefineGrating();
        DefineTrophyScoring();
        DefineCombat();
        DefineTrollAndCyclops();
        DefineThief();
        DefineDamAndReservoir();
        DefineBoat();
        DefineMachineRoom();
        DefineLoudRoom();
        DefineEggAndCanary();
        DefineRainbowAndPot();
        DefineHades();
        DefineBatAndGarlic();
        DefineSandAndScarab();
        DefineSkeleton();
        DefineDomeAndRope();
        DefineExplorationScore();
        DefineMagicAndChimney();
        DefineStoneBarrowWin();
        DefineDaemons();
    }

    private void DefineMailboxAndLeaflet()
    {
        _b.On(Mailbox).Before(_b.Verbs.Open!, ctx =>
        {
            ctx.Say("Opening the small mailbox reveals a leaflet.");
            return VerbResult.Pass;
        });
        _b.On(Leaflet).Before(_b.Verbs.Take!, ctx =>
        {
            if (!Mailbox.Has(Attr.Open))
            {
                ctx.Say("The leaflet is inside the closed mailbox.");
                return VerbResult.Done;
            }
            return VerbResult.Pass;
        });
    }

    private void DefineHouseEntry()
    {
        _b.On(Rug).Before(_move, ctx =>
        {
            if (!ctx.Get(_rugMoved))
            {
                ctx.Set(_rugMoved, true);
                TrapDoor.Set(Attr.Concealed, false);
                ctx.Say("With a great effort, you slide the rug aside, revealing a closed trap door.");
                return VerbResult.Done;
            }
            ctx.Say("The rug is already moved.");
            return VerbResult.Done;
        });

        _b.On(TrapDoor).Before(_b.Verbs.Open!, ctx =>
        {
            if (!ctx.Get(_rugMoved)) { ctx.Say("You can't see any door here."); return VerbResult.Done; }
            ctx.Set(_trapDoorOpen, true);
            return VerbResult.Pass;
        });
        _b.On(TrapDoor).Before(_b.Verbs.Close!, ctx => { ctx.Set(_trapDoorOpen, false); return VerbResult.Pass; });

        _b.On(KitchenWindow).Before(_b.Verbs.Open!, ctx => { ctx.Set(_windowOpen, true); return VerbResult.Pass; });
        _b.On(KitchenWindow).After(_b.Verbs.Open!, ctx =>
        {
            ctx.Say("With great effort, you open the window far enough to allow entry.");
            return VerbResult.Pass;
        });
        _b.On(KitchenWindow).Before(_b.Verbs.Close!, ctx => { ctx.Set(_windowOpen, false); return VerbResult.Pass; });
    }

    // ENGINE-LIMIT: ZorkSimplifications.Grating, GratingLight — no push-through; IsDark cleared manually on open.
    private void DefineGrating()
    {
        _b.On(Leaves).Before(_move, ctx =>
        {
            Grating.Set(Attr.Concealed, false);
            ctx.Say("Moving the leaves reveals a rusty grating.");
            return VerbResult.Done;
        });
        _b.On(Grating).After(_b.Verbs.Open!, ctx =>
        {
            ctx.Set(_gratingOpen, true);
            GratingRoom.IsDark = false;
            return VerbResult.Pass;
        });
        _b.On(Grating).After(_b.Verbs.Close!, ctx => { ctx.Set(_gratingOpen, false); return VerbResult.Pass; });
    }

    // ENGINE-LIMIT: ZorkSimplifications.Scoring — one-time Put bits; no score loss on take from case or death penalty.
    private void DefineTrophyScoring()
    {
        _b.Reactions.AfterAny(ctx =>
        {
            if (ctx.Verb != _b.Verbs.Put || ctx.IndirectObject != TrophyCase) return;
            ScoreIfInCase(ctx, Painting, 0, 6);
            ScoreIfInCase(ctx, JeweledEgg, 1, 5);
            ScoreIfInCase(ctx, GoldenCanary, 2, 4);
            ScoreIfInCase(ctx, BrassBauble, 3, 1);
            ScoreIfInCase(ctx, BagOfCoins, 4, 5);
            ScoreIfInCase(ctx, PlatinumBar, 5, 5);
            ScoreIfInCase(ctx, TrunkOfJewels, 6, 5);
            ScoreIfInCase(ctx, CrystalTrident, 7, 11);
            ScoreIfInCase(ctx, IvoryTorch, 8, 6);
            ScoreIfInCase(ctx, GoldCoffin, 9, 15);
            ScoreIfInCase(ctx, Sceptre, 10, 6);
            ScoreIfInCase(ctx, CrystalSkull, 11, 10);
            ScoreIfInCase(ctx, LargeEmerald, 12, 10);
            ScoreIfInCase(ctx, Scarab, 13, 5);
            ScoreIfInCase(ctx, PotOfGold, 14, 10);
            ScoreIfInCase(ctx, JadeFigurine, 15, 5);
            ScoreIfInCase(ctx, SapphireBracelet, 16, 5);
            ScoreIfInCase(ctx, HugeDiamond, 17, 10);
            ScoreIfInCase(ctx, SilverChalice, 18, 5);
        });
    }

    private void ScoreIfInCase(VerbContext ctx, Thing treasure, int bit, int points)
    {
        if (ctx.State.ContentsOf(TrophyCase).Contains(treasure))
            AwardTreasure(ctx, bit, points, $"You have gained {points} points.");
    }

    // ENGINE-LIMIT: ZorkSimplifications.Combat — instant sword kill; no melee rounds, disarm, or wake-on-leave.
    private void DefineCombat()
    {
        _b.On(Troll).Before(_attack, ctx => FightCreature(ctx, Troll, BloodyAxe, _trollDefeated,
            "The troll swings his axe, but you parry with your sword and run him through. The troll collapses."));
        _b.On(Cyclops).Before(_attack, ctx =>
        {
            if (ctx.Get(_cyclopsDead)) { ctx.Say("The cyclops is already dead."); return VerbResult.Done; }
            if (HasSword(ctx))
            {
                ctx.Set(_cyclopsDead, true);
                ctx.Remove(Cyclops);
                ctx.Say("The cyclops shrinks into a little pile of salt.");
                return VerbResult.Done;
            }
            ctx.Say("The cyclops catches your arm and nearly breaks it.");
            return VerbResult.Done;
        });
        _b.On(Thief).Before(_attack, ctx =>
        {
            if (ctx.Get(_thiefDead)) { ctx.Say("The thief is already dead."); return VerbResult.Done; }
            if (HasSword(ctx))
            {
                ctx.Set(_thiefDead, true);
                ctx.Remove(Thief);
                ctx.Say("The thief falls to the floor, dead. His bag spills open.");
                DropThiefLoot(ctx);
                return VerbResult.Done;
            }
            ctx.Say("The thief dodges your blow and laughs.");
            return VerbResult.Done;
        });
    }

    private VerbResult FightCreature(VerbContext ctx, Thing creature, Thing? droppedWeapon, StateKey<bool> defeatedFlag, string winMsg)
    {
        if (ctx.Get(defeatedFlag)) { ctx.Say("It's already dead."); return VerbResult.Done; }
        if (!HasSword(ctx)) { ctx.Say("You don't have a weapon!"); return VerbResult.Done; }
        ctx.Set(defeatedFlag, true);
        ctx.Remove(creature);
        if (droppedWeapon is not null) ctx.PlaceHere(droppedWeapon);
        ctx.Say(winMsg);
        return VerbResult.Done;
    }

    // ENGINE-LIMIT: ZorkSimplifications.Cyclops — lunch/bottle/yell sleep cyclops; no mood daemon or water quantity.
    private void DefineTrollAndCyclops()
    {
        _b.On(Troll).Before(_b.Verbs.Give!, ctx =>
        {
            if (ctx.DirectObject == Lunch || ctx.DirectObject == Garlic)
            {
                ctx.Remove(ctx.DirectObject);
                ctx.Set(_trollDefeated, true);
                ctx.Remove(Troll);
                ctx.Say("The troll takes the offering and falls over, dead.");
                return VerbResult.Done;
            }
            ctx.Say("The troll is not interested.");
            return VerbResult.Done;
        });

        _b.On(Cyclops).Before(_b.Verbs.Give!, ctx =>
        {
            if (ctx.DirectObject == Lunch)
            {
                ctx.Remove(Lunch);
                ctx.Say("The cyclops takes the lunch and devours it. He seems less hostile.");
                return VerbResult.Done;
            }
            if (ctx.DirectObject == Bottle && ctx.Carrying(Bottle))
            {
                ctx.Set(_cyclopsAsleep, true);
                ctx.Say("The cyclops drinks the water and falls fast asleep.");
                return VerbResult.Done;
            }
            ctx.Say("The cyclops is not interested.");
            return VerbResult.Done;
        });

        _b.On(Cyclops).Before(_yell, ctx =>
        {
            if (ctx.Get(_cyclopsDead)) return VerbResult.Pass;
            if (ctx.DirectObjectText?.Contains("odysseus", StringComparison.OrdinalIgnoreCase) == true ||
                ctx.DirectObjectText?.Contains("ulysseus", StringComparison.OrdinalIgnoreCase) == true)
            {
                ctx.Set(_cyclopsAsleep, true);
                ctx.Say("The cyclops, hearing the name of his father's nemesis, falls into a deep sleep.");
                return VerbResult.Done;
            }
            ctx.Set(_cyclopsAsleep, true);
            ctx.Say("The cyclops covers his ears and falls asleep.");
            return VerbResult.Done;
        });
    }

    // ENGINE-LIMIT: ZorkSimplifications.Thief — random steal + teleport; no bag, roaming, or sacred rooms.
    private void DefineThief()
    {
        _b.EveryTurn(ctx =>
        {
            if (ctx.Get(_thiefDead) || ctx.State.TurnCount < 20) return;
            if (ctx.Random.Next(6) != 0) return;

            if (!ctx.Here(Thief) && !ctx.Get(_thiefDead))
            {
                ctx.PlaceHere(Thief);
                ctx.Say("You catch a glimpse of a suspicious-looking figure slipping into the room.");
            }

            Thing? stolen = ctx.State.Inventory
                .Where(t => IsTreasure(t) && t != Sword && t != Lantern)
                .OrderBy(_ => ctx.Random.Next()).FirstOrDefault();
            if (stolen is null) return;
            ctx.Remove(stolen);
            ctx.State.Move(stolen, Placement.InRoom(TreasureRoom));
            ctx.Say("You hear a rustling in the darkness. Something has been taken from your pack!");
        }, when: ctx => !ctx.Get(_thiefDead));

        Thief.OnExamine = ctx =>
        {
            if (ctx.Get(_thiefDead)) return;
            ctx.Say("The thief is eyeing your possessions greedily.");
        };
    }

    private void DropThiefLoot(VerbContext ctx)
    {
        foreach (Thing t in ctx.State.ContentsOf(TreasureRoom).Where(IsTreasure).ToList())
            if (!ctx.State.ContentsOf(TrophyCase).Contains(t))
                ctx.PlaceHere(t);
    }

    // ENGINE-LIMIT: ZorkSimplifications.Dam — bolt turn instantly drains reservoir; no bubble/buttons or flood timer.
    private void DefineDamAndReservoir()
    {
        _b.On(Bolt).Before(_turn, ctx =>
        {
            if (!ctx.Carrying(Wrench)) { ctx.Say("You can't turn the bolt with your bare hands."); return VerbResult.Done; }
            if (ctx.Get(_damOpened)) { ctx.Say("The bolt is already turned."); return VerbResult.Done; }
            ctx.Set(_damOpened, true);
            ctx.Set(_lowTide, true);
            TrunkOfJewels.Set(Attr.Concealed, false);
            ctx.Say("The bolt turns with a squeak. Water pours through the dam. The reservoir drains.");
            ctx.State.Score += 4;
            return VerbResult.Done;
        });
    }

    // ENGINE-LIMIT: ZorkSimplifications.BoatAndRiver — inflate/deflate only; river rooms not gated on boat state.
    private void DefineBoat()
    {
        _b.On(PileOfPlastic).Before(_inflate, ctx =>
        {
            if (!ctx.Carrying(Pump)) { ctx.Say("You need a pump to inflate it."); return VerbResult.Done; }
            ctx.Remove(PileOfPlastic);
            ctx.Take(Boat);
            ctx.Set(_boatInflated, true);
            Boat.Description = "The boat is inflated and ready for use.";
            ctx.Say("The pile of plastic inflates into a serviceable boat.");
            return VerbResult.Done;
        });
        _b.On(Boat).Before(_deflate, ctx =>
        {
            if (!ctx.Get(_boatInflated)) { ctx.Say("It's already deflated."); return VerbResult.Done; }
            ctx.Set(_boatInflated, false);
            ctx.Say("The boat deflates.");
            return VerbResult.Done;
        });
    }

    private void DefineMachineRoom()
    {
        _b.On(Machine).Before(_b.Verbs.Put!, ctx =>
        {
            if (ctx.DirectObject != Coal || ctx.IndirectObject != Machine) return VerbResult.Pass;
            if (ctx.Get(_coalProcessed)) { ctx.Say("The machine has already done its work."); return VerbResult.Done; }
            if (!ctx.Carrying(Screwdriver)) { ctx.Say("You can't operate the machine without a screwdriver."); return VerbResult.Done; }
            ctx.Remove(Coal);
            ctx.Set(_coalProcessed, true);
            HugeDiamond.Set(Attr.Concealed, false);
            ctx.PlaceHere(HugeDiamond);
            ctx.Say("The machine rumbles. When the smoke clears, a huge diamond rests on the floor.");
            return VerbResult.Done;
        });
    }

    // ENGINE-LIMIT: ZorkSimplifications.LoudRoom — no per-room output filter; see EchoHandler instead.
    private void DefineLoudRoom() { }

    // ENGINE-LIMIT: ZorkSimplifications.Egg — open egg releases canary; no break-on-drop from tree.
    private void DefineEggAndCanary()
    {
        _b.On(JeweledEgg).Before(_b.Verbs.Open!, ctx =>
        {
            if (ctx.Get(_eggBroken)) return VerbResult.Pass;
            ctx.Set(_eggBroken, true);
            ctx.PlaceHere(GoldenCanary);
            ctx.Say("Opening the egg reveals a golden clockwork canary.");
            return VerbResult.Pass;
        });

        _b.On(GoldenCanary).Before(_wind, ctx =>
        {
            if (ctx.InRoom(ForestPath) && !ctx.Here(BrassBauble))
            {
                ctx.PlaceHere(BrassBauble);
                ctx.Say("The canary chirps, flutters, and drops a brass bauble at your feet.");
                return VerbResult.Done;
            }
            ctx.Say("The canary chirps weakly.");
            return VerbResult.Done;
        });
    }

    private void DefineRainbowAndPot()
    {
        _b.On(Sceptre).Before(_wave, ctx =>
        {
            if (ctx.InRoom(EndOfRainbow) || ctx.InRoom(R(ZorkIds.AragainFalls)))
            {
                ctx.Set(_rainbowSolid, true);
                PotOfGold.Set(Attr.Concealed, false);
                ctx.Say("The rainbow becomes solid and a pot of gold appears!");
                return VerbResult.Done;
            }
            ctx.Say("You wave the sceptre. Nothing happens.");
            return VerbResult.Done;
        });
    }

    // ENGINE-LIMIT: ZorkSimplifications.Hades — bell+book+candles in inventory; no ordered ritual sequence.
    private void DefineHades()
    {
        _b.On(BrassBell).Before(_ring, ctx =>
        {
            if (ctx.InRoom(EntranceToHades) && ctx.Carrying(BlackBook) && ctx.Carrying(PairOfCandles))
            {
                ctx.Set(_hadesOpen, true);
                ctx.Say("The bell tolls. The gate shimmers and opens.");
                return VerbResult.Done;
            }
            ctx.Say("The bell makes a hollow sound.");
            return VerbResult.Done;
        });
    }

    // ENGINE-LIMIT: ZorkSimplifications.Bat — instant death without garlic; no bat steal/repel loop.
    private void DefineBatAndGarlic()
    {
        BatRoom.OnEnter = ctx =>
        {
            if (ctx.Carrying(Garlic) || ctx.Wearing(Garlic)) return;
            ctx.Lose("A bat swoops down and carries you off into the darkness.");
        };
    }

    // ENGINE-LIMIT: ZorkSimplifications.Sand — four digs reveal scarab; fifth-dig collapse death omitted.
    private void DefineSandAndScarab()
    {
        _b.On(Sand).Before(_dig, ctx =>
        {
            if (ctx.IndirectObject != Shovel && ctx.DirectObject != Shovel && !ctx.Carrying(Shovel))
            {
                ctx.Say("Digging with your hands is slow and unrewarding.");
                return VerbResult.Done;
            }
            int digs = ctx.Get(_sandDigs) + 1;
            ctx.Set(_sandDigs, digs);
            if (digs < 4) { ctx.Say("You dig a hole in the sand."); return VerbResult.Done; }
            Scarab.Set(Attr.Concealed, false);
            ctx.PlaceHere(Scarab);
            ctx.Say("You uncover a beautiful jeweled scarab!");
            return VerbResult.Done;
        });
    }

    // ENGINE-LIMIT: ZorkSimplifications.Death — curse banishes treasures on take attempt; no undead/revival mode.
    private void DefineSkeleton()
    {
        _b.On(Skeleton).Before(_b.Verbs.Take!, ctx =>
        {
            ctx.Say("A ghost appears and curses your valuables, banishing them to the Land of the Dead!");
            foreach (Thing t in ctx.State.Inventory.Where(IsTreasure).ToList())
                ctx.State.Move(t, Placement.InRoom(LandOfTheDead));
            return VerbResult.Done;
        });
    }

    private void DefineDomeAndRope()
    {
        _b.On(Rope).Before(_tie, ctx =>
        {
            if (ctx.InRoom(DomeRoom) || ctx.IndirectObject?.Name.Contains("railing", StringComparison.OrdinalIgnoreCase) == true)
            {
                ctx.Set(_domeRopeTied, true);
                ctx.Say("The rope is tied to the railing.");
                return VerbResult.Done;
            }
            return VerbResult.Pass;
        });
    }

    // ENGINE-LIMIT: ZorkSimplifications.Scoring — place bonuses only; task scores and death penalty omitted.
    private void DefineExplorationScore()
    {
        Kitchen.OnFirstEnter = ctx => AwardPlaceScore(ctx, 0, 10);
        Cellar.OnFirstEnter = ctx => { AwardPlaceScore(ctx, 1, 25); ctx.Say("You have entered the cellar. (+25)"); };
        TreasureRoom.OnFirstEnter = ctx => AwardPlaceScore(ctx, 2, 25);
        EwPassage.OnFirstEnter = ctx => AwardPlaceScore(ctx, 3, 5);
        DraftyRoom.OnFirstEnter = ctx => AwardPlaceScore(ctx, 4, 13);
    }

    // ENGINE-LIMIT: ZorkSimplifications.MagicPassage — flags on first-enter; cyclops wall break not wired.
    private void DefineMagicAndChimney()
    {
        Gallery.OnFirstEnter = ctx => ctx.Set(_magicFlag, true);
        R(ZorkIds.Studio).OnFirstEnter = ctx => ctx.Set(_chimneyFlag, true);
        CyclopsRoom.OnEnter = ctx =>
        {
            if (ctx.Get(_cyclopsDead) || ctx.Get(_cyclopsAsleep)) return;
            ctx.Say("The cyclops blocks the staircase.");
        };
    }

    private void DefineStoneBarrowWin()
    {
        StoneBarrow.OnEnter = ctx =>
        {
            if (!ctx.Get(_wonFlag))
            {
                ctx.Say("The barrow is sealed.");
                return;
            }
            ctx.Win("{fg:gold}{bold}Inside the barrow you discover the final secret of the Great Underground Empire. " +
                    "You have mastered Zork!{/}{/}");
        };
    }

    private void DefineDaemons()
    {
        _b.EveryTurn(ctx =>
        {
            if (!Lantern.Has(Attr.Lit)) return;
            int left = ctx.Get(_lanternTurns);
            if (left <= 0)
            {
                Lantern.Set(Attr.Lit, false);
                ctx.Say("Your lantern flickers and goes out.");
                return;
            }
            ctx.Set(_lanternTurns, left - 1);
            if (left == 20) ctx.Say("Your lantern is getting dim.");
        });

        _b.EveryTurn(ctx =>
        {
            if (!InDarkWithoutLight(ctx)) { ctx.Set(_grueTurns, 0); return; }
            int turns = ctx.Get(_grueTurns) + 1;
            ctx.Set(_grueTurns, turns);
            if (turns >= 3) ctx.Lose("It is pitch black. You are likely to be eaten by a grue.");
        });

        // ENGINE-LIMIT: ZorkSimplifications.Encumbrance — altar-only block; no global carry weight.
        _b.On(GoldCoffin).Before(_b.Verbs.Take!, ctx =>
        {
            if (ctx.InRoom(Altar) || ctx.State.RoomOf(GoldCoffin) == Altar)
            {
                ctx.Say("The coffin is too heavy to lift from the altar.");
                return VerbResult.Done;
            }
            return VerbResult.Pass;
        });
    }

    // ---- verb handlers --------------------------------------------------------------------

    private VerbResult GoHandler(VerbContext ctx)
    {
        if (ctx.Direction is not { } dir)
        {
            ctx.Say("Go where?");
            return VerbResult.Done;
        }

        Exit? exit = ctx.CurrentRoom.ExitTo(dir);
        if (exit is null)
        {
            ctx.Say("You can't go that way.");
            return VerbResult.Done;
        }

        if (!exit.CanPass(ctx, out string? reason))
        {
            ctx.Say(reason ?? "You can't go that way.");
            return VerbResult.Done;
        }

        ctx.MovePlayerTo(exit.Destination);
        return VerbResult.Done;
    }

    private VerbResult RingHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == BrassBell) return VerbResult.Pass;
        ctx.Say("Ring what?");
        return VerbResult.Done;
    }

    private bool HasSword(GameContext ctx) => ctx.Carrying(Sword) || ctx.Wearing(Sword);

    private VerbResult AttackHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null) { ctx.Say("Attack what?"); return VerbResult.Done; }
        if (ctx.DirectObject == Troll || ctx.DirectObject == Cyclops || ctx.DirectObject == Thief) return VerbResult.Pass;
        ctx.Say("Violence isn't the answer to every problem.");
        return VerbResult.Done;
    }

    private VerbResult MoveHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null) { ctx.Say("Move what?"); return VerbResult.Done; }
        if (ctx.DirectObject == Rug || ctx.DirectObject == Leaves) return VerbResult.Pass;
        ctx.Say("You can't move that.");
        return VerbResult.Done;
    }

    private VerbResult ClimbHandler(VerbContext ctx)
    {
        if ((ctx.DirectObject is null && ctx.InRoom(ForestPath)) ||
            ctx.DirectObject?.Name.Contains("tree", StringComparison.OrdinalIgnoreCase) == true)
        {
            ctx.MovePlayerTo(UpATree);
            return VerbResult.Done;
        }
        ctx.Say("You can't climb that.");
        return VerbResult.Done;
    }

    private VerbResult InflateHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == PileOfPlastic || ctx.DirectObject == Boat) return VerbResult.Pass;
        ctx.Say("You can't inflate that.");
        return VerbResult.Done;
    }

    private VerbResult DeflateHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == Boat) return VerbResult.Pass;
        ctx.Say("You can't deflate that.");
        return VerbResult.Done;
    }

    // ENGINE-LIMIT: ZorkSimplifications.LoudRoom — only ECHO garbles text; original garbles all commands.
    private VerbResult EchoHandler(VerbContext ctx)
    {
        if (!ctx.InRoom(LoudRoom)) { ctx.Say("You hear nothing special."); return VerbResult.Done; }
        string word = ctx.DirectObjectText ?? ctx.DirectObject?.Name ?? "hello";
        if (word.Contains("bar", StringComparison.OrdinalIgnoreCase))
        {
            PlatinumBar.Set(Attr.Concealed, false);
            ctx.PlaceHere(PlatinumBar);
            ctx.Say("bar bar bar ... A platinum bar appears!");
        }
        else ctx.Say($"{word} ... {word} ... {word} ...");
        return VerbResult.Done;
    }

    private VerbResult YellHandler(VerbContext ctx)
    {
        if (ctx.InRoom(CyclopsRoom) && !ctx.Get(_cyclopsDead)) return VerbResult.Pass;
        ctx.Say("You yell loudly.");
        return VerbResult.Done;
    }

    // ENGINE-LIMIT: ZorkSimplifications.Dam — maintenance blast = instant death only; no button puzzle.
    private VerbResult BlastHandler(VerbContext ctx)
    {
        if (ctx.InRoom(MaintenanceRoom) || ctx.DirectObject?.Name.Contains("panel", StringComparison.OrdinalIgnoreCase) == true)
        {
            ctx.Lose("The dam explodes. You are blown to bits.");
            return VerbResult.Done;
        }
        ctx.Say("Blast what?");
        return VerbResult.Done;
    }

    private VerbResult TurnHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == Bolt) return VerbResult.Pass;
        ctx.Say("Turn what?");
        return VerbResult.Done;
    }

    private VerbResult WindHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == GoldenCanary) return VerbResult.Pass;
        ctx.Say("Wind what?");
        return VerbResult.Done;
    }

    private VerbResult DigHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == Sand || ctx.InRoom(SandyCave)) return VerbResult.Pass;
        ctx.Say("Dig what?");
        return VerbResult.Done;
    }

    private VerbResult PrayHandler(VerbContext ctx)
    {
        ctx.Say("You achieve a brief moment of serenity.");
        return VerbResult.Done;
    }

    private VerbResult WaveHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == Sceptre) return VerbResult.Pass;
        ctx.Say("Wave what?");
        return VerbResult.Done;
    }

    private VerbResult TieHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == Rope) return VerbResult.Pass;
        ctx.Say("Tie what?");
        return VerbResult.Done;
    }

    private VerbResult TempleHandler(VerbContext ctx)
    {
        if (ctx.InRoom(Temple)) ctx.MovePlayerTo(TreasureRoom);
        else if (ctx.InRoom(TreasureRoom)) ctx.MovePlayerTo(Temple);
        else ctx.Say("Nothing happens.");
        return VerbResult.Done;
    }

    private static bool IsTreasure(Thing thing) =>
        thing.Name is "painting" or "jewel-encrusted egg" or "golden clockwork canary" or "brass bauble"
            or "leather bag of coins" or "platinum bar" or "trunk of jewels" or "crystal trident"
            or "ivory torch" or "gold coffin" or "sceptre" or "crystal skull" or "large emerald"
            or "beautiful jeweled scarab" or "pot of gold" or "jade figurine"
            or "sapphire-encrusted bracelet" or "huge diamond" or "silver chalice";
}
