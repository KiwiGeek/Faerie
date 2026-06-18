using Faerie.Building;
using Faerie.Model;
using Faerie.Presentation;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Zork;

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
    // Go omits "move" so "move north" does not travel; custom move (registered after AddCoreVerbs) overrides core push for object commands.
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
        _echo = _b.DefineVerb("echo", ["echo"], VerbForms.Intransitive | VerbForms.Transitive, EchoHandler);
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
        DefineSwordGlow();
        DefineTrollAndCyclops();
        DefineCyclops();
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

    // ENGINE-LIMIT: ZorkSimplifications.Grating — no push-through from clearing.
    private void DefineGrating()
    {
        GratingRoom.LitWhen(ctx => ctx.Get(_gratingOpen));

        _b.On(Leaves).Before(_move, ctx =>
        {
            Grating.Set(Attr.Concealed, false);
            ctx.Say("Moving the leaves reveals a rusty grating.");
            return VerbResult.Done;
        });
        _b.On(Grating).After(_b.Verbs.Open!, ctx =>
        {
            ctx.Set(_gratingOpen, true);
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

    // Full melee combat: attacking a villain trades blows over several turns. Either side can miss,
    // wound, knock the other senseless, disarm, or kill. Your offense is the 'attack' verb; the
    // villain's offense (plus recovery and your slow healing) runs each turn in CombatRound. An
    // unconscious villain can be finished off, revives after a few turns, or wakes if you leave.
    private const int PlayerMaxHp = 5;

    private void DefineCombat()
    {
        _b.On(Troll).Before(_attack, ctx => PlayerStrikes(ctx, Troll, _trollHp, _trollKO, _trollDefeated, "troll", KillTroll));
        _b.On(Thief).Before(_attack, ctx => PlayerStrikes(ctx, Thief, _thiefHp, _thiefKO, _thiefDead, "thief", KillThief));

        _b.EveryTurn(CombatRound);
    }

    private void KillTroll(GameContext ctx)
    {
        ctx.Set(_trollDefeated, true);
        ctx.Remove(Troll);
        ctx.PlaceHere(BloodyAxe);
        ctx.Say("The troll lets out a strangled cry and collapses. His bloody axe clatters to the floor.");
    }

    private void KillThief(GameContext ctx)
    {
        ctx.Set(_thiefDead, true);
        ctx.Remove(Thief);
        ctx.Say("The thief falls to the floor, dead. His bag spills open, scattering its contents.");
        DropThiefLoot(ctx);
    }

    // ---- your offense (the 'attack' verb) -------------------------------------------------

    private VerbResult PlayerStrikes(VerbContext ctx, Thing villain, StateKey<int> hp, StateKey<int> ko,
        StateKey<bool> dead, string name, Action<GameContext> kill)
    {
        if (ctx.Get(dead)) { ctx.Say($"The {name} is already dead."); return VerbResult.Done; }
        if (!ctx.Here(villain)) { ctx.Say($"There is no {name} here."); return VerbResult.Done; }

        Thing? weapon = ChosenWeapon(ctx);
        int power = PowerOf(weapon);
        if (power == 0)
        {
            ctx.Say($"Attacking the {name} with your bare hands is a hopeless proposition.");
            return VerbResult.Done;
        }
        string with = $" with the {weapon!.Name}";

        // A senseless villain is defenceless: finish him.
        if (ctx.Get(ko) > 0) { kill(ctx); return VerbResult.Done; }

        int roll = ctx.Random.Next(100) + power * 12;   // a finer weapon tips the odds your way
        if (roll >= 92)
        {
            ctx.SayInline($"A masterful stroke{with}! ");
            kill(ctx);
        }
        else if (roll >= 74)
        {
            ctx.Set(ko, 2 + ctx.Random.Next(3));
            ctx.Say($"You catch the {name} square on the head{with}. He crumples senseless to the floor.");
        }
        else if (roll >= 40)
        {
            int left = ctx.Get(hp) - 1;
            ctx.Set(hp, left);
            if (left <= 0) kill(ctx);
            else ctx.Say($"Your blow lands{with}. The {name} is wounded, but fights on.");
        }
        else
        {
            ctx.Say($"The {name} parries your attack{with} and counters.");
        }
        return VerbResult.Done;
    }

    // ---- the villains' offense, recovery, and your slow healing ---------------------------

    private void CombatRound(GameContext ctx)
    {
        VillainTurn(ctx, Troll, _trollKO, _trollDefeated, "troll", "axe");
        if (ctx.State.IsOver) return;
        VillainTurn(ctx, Thief, _thiefKO, _thiefDead, "thief", "stiletto");
        if (ctx.State.IsOver) return;

        if (!HostilePresent(ctx))
        {
            int hp = ctx.Get(_playerHp);
            if (hp < PlayerMaxHp) ctx.Set(_playerHp, hp + 1);
        }
    }

    private void VillainTurn(GameContext ctx, Thing villain, StateKey<int> ko, StateKey<bool> dead,
        string name, string weapon)
    {
        if (ctx.Get(dead)) return;

        int koLeft = ctx.Get(ko);
        if (koLeft > 0)
        {
            // Leave him senseless and he comes to off-stage; stay, and he slowly revives.
            if (!ctx.Here(villain)) { ctx.Set(ko, 0); return; }
            ctx.Set(ko, --koLeft);
            if (koLeft == 0) ctx.Say($"The {name} stirs, then climbs groggily back to his feet.");
            return;
        }

        if (!ctx.Here(villain)) return;

        Thing? yours = BestWeapon(ctx);
        int roll = ctx.Random.Next(100) + (yours is null ? 18 : 0);   // unarmed, you're easier prey
        if (roll >= 94)
        {
            ctx.Set(_playerHp, 0);
            ctx.Lose($"The {name}'s {weapon} finds its mark, and the world goes black. You have died.");
        }
        else if (roll >= 70)
        {
            Wound(ctx, 2, $"The {name}'s {weapon} bites deep. You are badly wounded.");
        }
        else if (roll >= 46)
        {
            Wound(ctx, 1, $"The {name}'s {weapon} grazes you.");
        }
        else if (roll >= 34 && yours is not null)
        {
            ctx.Remove(yours);
            ctx.PlaceHere(yours);
            ctx.Say($"The {name} knocks the {yours.Name} from your grasp! It falls to the floor.");
        }
        else
        {
            ctx.Say($"The {name} swings at you and misses.");
        }
    }

    private void Wound(GameContext ctx, int amount, string message)
    {
        int left = ctx.Get(_playerHp) - amount;
        ctx.Set(_playerHp, left);
        if (left <= 0) ctx.Lose("Your wounds are too grave. You sink to the ground, and your adventure ends here.");
        else ctx.Say(message);
    }

    // ---- weapon selection -----------------------------------------------------------------

    private Thing? ChosenWeapon(VerbContext ctx) =>
        ctx.IndirectObject is { } io && IsWeapon(io) && ctx.Carrying(io) ? io : BestWeapon(ctx);

    private Thing? BestWeapon(GameContext ctx) =>
        ctx.Carrying(Sword) ? Sword :
        ctx.Carrying(NastyKnife) ? NastyKnife :
        ctx.Carrying(RustyKnife) ? RustyKnife :
        ctx.Carrying(BloodyAxe) ? BloodyAxe : null;

    private bool IsWeapon(Thing t) => t == Sword || t == NastyKnife || t == RustyKnife || t == BloodyAxe;

    private int PowerOf(Thing? weapon) => weapon is null ? 0 : weapon == Sword ? 2 : 1;

    private bool HostilePresent(GameContext ctx) =>
        (!ctx.Get(_trollDefeated) && ctx.Get(_trollKO) == 0 && ctx.Here(Troll)) ||
        (!ctx.Get(_thiefDead) && ctx.Get(_thiefKO) == 0 && ctx.Here(Thief));

    // The elvish sword warns of nearby danger: a faint glow when a living villain is one room
    // away, a bright glow when one shares the room. Only reports on change so it never spams.
    private void DefineSwordGlow()
    {
        _b.EveryTurn(ctx =>
        {
            int level = SwordGlowLevel(ctx);
            if (level == ctx.Get(_swordGlow)) return;
            ctx.Set(_swordGlow, level);
            ctx.Say(level switch
            {
                2 => "Your sword is glowing very brightly.",
                1 => "Your sword is glowing with a faint blue glow.",
                _ => "Your sword is no longer glowing.",
            });
        }, when: ctx => ctx.Here(Sword));
    }

    private int SwordGlowLevel(GameContext ctx)
    {
        int Level(Thing villain, bool defeated)
        {
            if (defeated) return 0;
            Room? room = ctx.RoomOf(villain);
            if (room is null) return 0;
            if (room == ctx.CurrentRoom) return 2;
            return ctx.IsAdjacent(room) ? 1 : 0;
        }

        return Math.Max(Level(Troll, ctx.Get(_trollDefeated)), Level(Thief, ctx.Get(_thiefDead)));
    }

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
    }

    // ENGINE-LIMIT: ZorkSimplifications.Thief — see ZorkWorld.Thief.cs (I-THIEF daemon).
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

    // The Loud Room throws every line of output back at you until you master its acoustics (say ECHO).
    // Implemented with the engine's output-filter seam (GameBuilder.FilterOutput).
    private void DefineLoudRoom()
    {
        _b.FilterOutput((ctx, text) =>
            ctx.InRoom(LoudRoom) && !LoudRoomQuiet(ctx) && LastWord(text) is { } word
                ? $"{text}\n{{fg:darkgray}}{word}... {word}...{{/}}"
                : text);
    }

    /// <summary>The last run of letters in a (possibly marked-up) line, or null if there is none.</summary>
    private static string? LastWord(string text)
    {
        string plain = Markup.Strip(text).Trim();
        if (plain.Length == 0 || plain.All(static c => c is '>' or ' '))
            return null;

        System.Text.RegularExpressions.MatchCollection words =
            System.Text.RegularExpressions.Regex.Matches(plain, "[A-Za-z]+");
        return words.Count == 0 ? null : words[^1].Value;
    }

    private void DefineEggAndCanary()
    {
        JeweledEgg.OnDrop = ctx =>
        {
            if (!ctx.InRoom(UpATree) || ctx.Get(_eggBroken)) return false;
            ctx.Set(_eggBroken, true);
            ctx.Remove(JeweledEgg);
            ctx.PlaceHere(GoldenCanary);
            ctx.Say("You let go of the egg and it falls to the ground with a sickening crunch. " +
                    "The egg breaks open, revealing a golden clockwork canary.");
            return true;
        };

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
            if (!ctx.InRoom(DomeRoom)) return VerbResult.Pass;
            if (ctx.IndirectObject is not null && ctx.IndirectObject != DomeRailing) return VerbResult.Pass;

            ctx.Set(_domeRopeTied, true);
            ctx.Say("The rope is tied to the railing.");
            return VerbResult.Done;
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

        if (!exit.TryTraverse(ctx, out string? reason, out bool shouldMove))
        {
            ctx.Say(reason ?? "You can't go that way.");
            return VerbResult.Done;
        }

        if (shouldMove)
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
        if (ctx.DirectObject == Troll || ctx.DirectObject == Cyclops || ctx.DirectObject == Thief)
            return VerbResult.Pass;
        ctx.Say("Violence isn't the answer to every problem.");
        return VerbResult.Done;
    }

    private VerbResult MoveHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null) { ctx.Say("Move what?"); return VerbResult.Done; }
        if (ctx.DirectObject == Rug || ctx.DirectObject == Leaves ||
            ctx.DirectObject == BlueButton || ctx.DirectObject == YellowButton ||
            ctx.DirectObject == BrownButton || ctx.DirectObject == RedButton)
            return VerbResult.Pass;
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

        // Mastering the acoustics silences the room (stops the output-filter echo) before we speak.
        ctx.Set(_loudQuieted, true);
        PlatinumBar.Set(Attr.Concealed, false);
        ctx.PlaceHere(PlatinumBar);
        ctx.Say("The acoustics of the room change subtly.");
        return VerbResult.Done;
    }

    private VerbResult YellHandler(VerbContext ctx)
    {
        ctx.Say("You yell loudly.");
        return VerbResult.Done;
    }

    private VerbResult BlastHandler(VerbContext ctx)
    {
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
        if (TryAltarPray(ctx)) return VerbResult.Done;
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
