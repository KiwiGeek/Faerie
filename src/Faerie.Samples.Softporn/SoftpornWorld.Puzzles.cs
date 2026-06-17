using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;
using static Faerie.Verbs.StandardVerbIds;

namespace Faerie.Samples.Softporn;

internal sealed partial class SoftpornWorld
{
    private Verb _go = null!;
    private Verb _hail = null!;
    private Verb _fuck = null!;
    private Verb _marry = null!;
    private Verb _play = null!;
    private Verb _press = null!;
    private Verb _flush = null!;
    private Verb _inflate = null!;
    private Verb _call = null!;
    private Verb _answer = null!;
    private Verb _climb = null!;
    private Verb _enter = null!;
    private Verb _listen = null!;
    private Verb _jump = null!;
    private Verb _dance = null!;
    private Verb _smoke = null!;
    private Verb _kiss = null!;
    private Verb _stab = null!;
    private Verb _break = null!;
    private Verb _buy = null!;

    private Verb _pay = null!;
    private Verb _cut = null!;
    private Verb _water = null!;
    private Verb _fill = null!;
    private Verb _pour = null!;
    private Verb _smell = null!;
    private Verb _show = null!;

    private void DefineCustomVerbs()
    {
        _go = _b.DefineVerb(Go, ["go", "walk", "run", "head"],
            VerbForms.Transitive | VerbForms.Intransitive, GoHandler);

        _b.DefineVerb(Help, ["help", "?"], VerbForms.Intransitive, ctx =>
        {
            ShowHelp(ctx);
            return VerbResult.Done;
        });

        _b.DefineVerb(Score, ["score"], VerbForms.Intransitive, ctx =>
        {
            ctx.Blank();
            ctx.Say($"Your score is '{ctx.State.Score}' out of a possible '3'");
            return VerbResult.Done;
        });

        _hail = _b.DefineVerb("hail", ["hail"], VerbForms.Transitive, HailHandler);
        _fuck = _b.DefineVerb("fuck", ["fuck", "screw", "seduce"], VerbForms.Transitive, FuckHandler);
        _marry = _b.DefineVerb("marry", ["marry"], VerbForms.Transitive, MarryHandler);
        _play = _b.DefineVerb("play", ["play"], VerbForms.Transitive, PlayHandler);
        _press = _b.DefineVerb("press", ["press", "push"], VerbForms.Transitive, PressHandler);
        _flush = _b.DefineVerb("flush", ["flush"], VerbForms.Transitive, FlushHandler);
        _inflate = _b.DefineVerb("inflate", ["inflate", "blow"], VerbForms.Transitive, InflateHandler);
        _call = _b.DefineVerb("call", ["call", "dial"], VerbForms.Transitive | VerbForms.Intransitive, CallHandler);
        _answer = _b.DefineVerb("answer", ["answer"], VerbForms.Transitive, AnswerHandler);
        _climb = _b.DefineVerb("climb", ["climb"], VerbForms.Transitive, ClimbHandler);
        _enter = _b.DefineVerb("enter", ["enter"], VerbForms.Transitive, EnterHandler);
        _listen = _b.DefineVerb("listen", ["listen"], VerbForms.Transitive, ListenHandler);
        _jump = _b.DefineVerb("jump", ["jump"], VerbForms.Intransitive, JumpHandler);
        _dance = _b.DefineVerb("dance", ["dance"], VerbForms.Intransitive, DanceHandler);
        _smoke = _b.DefineVerb("smoke", ["smoke"], VerbForms.Transitive, SmokeHandler);
        _kiss = _b.DefineVerb("kiss", ["kiss"], VerbForms.Transitive, KissHandler);
        _stab = _b.DefineVerb("stab", ["stab"], VerbForms.Transitive, StabHandler);
        _break = _b.DefineVerb("break", ["break", "smash"], VerbForms.Transitive, _ => VerbResult.Pass);
        _buy = _b.DefineVerb("buy", ["buy", "order"], VerbForms.Transitive, BuyHandler);
        _pay = _b.DefineVerb("pay", ["pay"], VerbForms.Transitive, PayHandler);
        _cut = _b.DefineVerb("cut", ["cut"], VerbForms.Transitive, CutHandler);
        _water = _b.DefineVerb("water", ["water"], VerbForms.Transitive, WaterHandler);
        _fill = _b.DefineVerb("fill", ["fill"], VerbForms.Transitive, FillHandler);
        _pour = _b.DefineVerb("pour", ["pour"], VerbForms.Transitive | VerbForms.Ditransitive, PourHandler);
        _smell = _b.DefineVerb("smell", ["smell"], VerbForms.Transitive, SmellHandler);
        _show = _b.DefineVerb("show", ["show"], VerbForms.Transitive, ShowHandler);
    }

    private void DefinePuzzles()
    {
        DefineCarryLimit();
        DefineTakeRules();
        DefineDropGifts();
        DefineExamines();
        DefineContainers();
        DefineCommerce();
        DefineRomance();
        DefinePhone();
        DefineGarden();
        DefineDeathTraps();
        DefineNpcDrops();
        DefineSmellAndShow();
    }

    private void DefineDaemons()
    {
        _b.EveryTurn(ApplyAreaUpdates);
        _b.EveryTurn(ctx =>
        {
            if (ctx.InRoom(PenthousePorch) && ctx.Get(_called5550439) && !ctx.Get(_telephoneAnswered) &&
                ctx.Random.Next(4) == 2)
            {
                ctx.Set(_telephoneRinging, true);
                ctx.Say("The telephone rings");
            }
        });
        _b.EveryTurn(ctx =>
        {
            if (ctx.Get(_hookerFucked) && ctx.LocatedIn(T(SoftpornIds.Blonde), R(SoftpornIds.HotelDesk)))
                ctx.Remove(T(SoftpornIds.Blonde));
        });
    }

    private void DefineCarryLimit()
    {
        _b.Reactions.BeforeAny(ctx =>
        {
            if (ctx.Get(_tiedToBed) && ctx.Verb.Id is Go or "enter" or "hail")
            {
                ctx.Say("But I'm tied to the bed!!!!!");
                return VerbResult.Done;
            }

            if (ctx.Verb.Id is "attack" or "kill")
            {
                ctx.Say("Try using a knife!!!");
                return VerbResult.Done;
            }

            return VerbResult.Pass;
        });

        _b.Reactions.BeforeAny(ctx =>
        {
            if (ctx.Verb.Id != Take || ctx.DirectObject is not { } obj) return VerbResult.Pass;
            if (obj == T(SoftpornIds.Wallet)) return VerbResult.Pass;
            if (ctx.Carrying(obj)) return VerbResult.Pass;
            if (CarryCount(ctx) >= MaxCarried)
            {
                ctx.Say("I'm carrying too much!!!");
                return VerbResult.Done;
            }

            return VerbResult.Pass;
        });

        _b.Reactions.AfterAny(ctx =>
        {
            if (ctx.Verb.Id == Take && ctx.DirectObject is { } obj && obj != T(SoftpornIds.Wallet))
                IncrementCarry(ctx);
            if (ctx.Verb.Id == Drop && ctx.DirectObject is { } dropped && dropped != T(SoftpornIds.Wallet))
                DecrementCarry(ctx);
        });
    }

    // ---- verb handlers (continued in partial) ----

    private VerbResult GoHandler(VerbContext ctx)
    {
        if (ctx.Get(_tiedToBed))
        {
            ctx.Say("But I'm tied to the bed!!!!!");
            return VerbResult.Done;
        }

        if (ctx.Direction is not { } dir)
        {
            ctx.Say("Go where?");
            return VerbResult.Done;
        }

        if (ctx.CurrentRoom == HookerBalcony && dir == Direction.West && !ctx.Get(_ropeInUse))
        {
            FallingDown(ctx, jumped: false);
            return VerbResult.Done;
        }

        if (ctx.CurrentRoom == Backroom && dir == Direction.Up)
        {
            if (ctx.Get(_tvChannel) != 6)
            {
                ctx.Say("The Pimp says I can't until I get $2000");
                return VerbResult.Done;
            }

            if (!HasMoney(ctx, 20) || !CarryingWallet(ctx))
            {
                ctx.Say("The Pimp says I can't until I get $2000");
                return VerbResult.Done;
            }

            if (ctx.Get(_hookerFucked))
            {
                ctx.Say("The Pimp says 'No -- the hooker can't take it anymore!'");
                return VerbResult.Done;
            }

            ctx.Say("The Pimp takes $2000 and says OK");
            Spend(ctx, 20);
            ctx.MovePlayerTo(HookerBedroom);
            return VerbResult.Done;
        }

        Exit? exit = ctx.CurrentRoom.ExitTo(dir);
        if (exit is null)
        {
            ctx.Say("I can't go that way!");
            return VerbResult.Done;
        }

        if (!exit.CanPass(ctx, out string? reason))
        {
            ctx.Say(reason ?? "I can't go that way!");
            return VerbResult.Done;
        }

        ctx.MovePlayerTo(exit.Destination);
        return VerbResult.Done;
    }
}
