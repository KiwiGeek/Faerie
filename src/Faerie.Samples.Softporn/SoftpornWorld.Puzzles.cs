using Faerie.Model;
using Faerie.Verbs;
using static Faerie.Verbs.StandardVerbIds;

namespace Faerie.Samples.Softporn;

internal sealed partial class SoftpornWorld
{
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
        _softporn.DefineVerb(Help, ["help", "?"], VerbForms.Intransitive, ctx =>
        {
            ShowHelp(ctx);
            return VerbResult.Done;
        });

        _softporn.DefineVerb(Score, ["score"], VerbForms.Intransitive, ctx =>
        {
            ctx.Blank();
            ctx.Say($"Your score is '{ctx.State.Score}' out of a possible '3'");
            return VerbResult.Done;
        });

        _hail = _softporn.DefineVerb("hail", ["hail"], VerbForms.Transitive, HailHandler);
        _fuck = _softporn.DefineVerb("fuck", ["fuck", "screw", "seduce"], VerbForms.Transitive, FuckHandler);
        _marry = _softporn.DefineVerb("marry", ["marry"], VerbForms.Transitive, MarryHandler);
        _play = _softporn.DefineVerb("play", ["play"], VerbForms.Transitive, PlayHandler);
        _press = _softporn.DefineVerb("press", ["press", "push"], VerbForms.Transitive, PressHandler);
        _flush = _softporn.DefineVerb("flush", ["flush"], VerbForms.Transitive, FlushHandler);
        _inflate = _softporn.DefineVerb("inflate", ["inflate", "blow"], VerbForms.Transitive, InflateHandler);
        _call = _softporn.DefineVerb("call", ["call", "dial"], VerbForms.Transitive | VerbForms.Intransitive, CallHandler);
        _answer = _softporn.DefineVerb("answer", ["answer"], VerbForms.Transitive, AnswerHandler);
        _climb = _softporn.DefineVerb("climb", ["climb"], VerbForms.Transitive, ClimbHandler);
        _enter = _softporn.DefineVerb("enter", ["enter"], VerbForms.Transitive, EnterHandler);
        _listen = _softporn.DefineVerb("listen", ["listen"], VerbForms.Transitive, ListenHandler);
        _jump = _softporn.DefineVerb("jump", ["jump"], VerbForms.Intransitive, JumpHandler);
        _dance = _softporn.DefineVerb("dance", ["dance"], VerbForms.Intransitive, DanceHandler);
        _smoke = _softporn.DefineVerb("smoke", ["smoke"], VerbForms.Transitive, SmokeHandler);
        _kiss = _softporn.DefineVerb("kiss", ["kiss"], VerbForms.Transitive, KissHandler);
        _stab = _softporn.DefineVerb("stab", ["stab"], VerbForms.Transitive, StabHandler);
        _break = _softporn.DefineVerb("break", ["break", "smash"], VerbForms.Transitive, _ => VerbResult.Pass);
        _buy = _softporn.DefineVerb("buy", ["buy", "order"], VerbForms.Transitive, BuyHandler);
        _pay = _softporn.DefineVerb("pay", ["pay"], VerbForms.Transitive, PayHandler);
        _cut = _softporn.DefineVerb("cut", ["cut"], VerbForms.Transitive, CutHandler);
        _water = _softporn.DefineVerb("water", ["water"], VerbForms.Transitive, WaterHandler);
        _fill = _softporn.DefineVerb("fill", ["fill"], VerbForms.Transitive, FillHandler);
        _pour = _softporn.DefineVerb("pour", ["pour"], VerbForms.Transitive | VerbForms.Ditransitive, PourHandler);
        _smell = _softporn.DefineVerb("smell", ["smell"], VerbForms.Transitive, SmellHandler);
        _show = _softporn.DefineVerb("show", ["show"], VerbForms.Transitive, ShowHandler);
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
        _softporn.EveryTurn(ApplyAreaUpdates);
        _softporn.EveryTurn(ctx =>
        {
            if (ctx.InRoom(PenthousePorch) && ctx.Get(_called5550439) && !ctx.Get(_telephoneAnswered) &&
                ctx.Random.Next(4) == 2)
            {
                ctx.Set(_telephoneRinging, true);
                ctx.Say("The telephone rings");
            }
        });
        _softporn.EveryTurn(ctx =>
        {
            if (ctx.Get(_hookerFucked) && ctx.LocatedIn(T(Blonde), HotelDesk))
                ctx.Remove(T(Blonde));
        });
    }

    private void DefineCarryLimit()
    {
        _softporn.Reactions.BeforeAny(ctx =>
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

            if (ctx.Verb.Id != Take || ctx.DirectObject is not { } obj) return VerbResult.Pass;
            if (obj == T(Wallet)) return VerbResult.Pass;
            if (ctx.Carrying(obj)) return VerbResult.Pass;
            if (CarryCount(ctx) >= MaxCarried)
            {
                ctx.Say("I'm carrying too much!!!");
                return VerbResult.Done;
            }

            return VerbResult.Pass;
        });

        _softporn.Reactions.AfterAny(ctx =>
        {
            if (ctx.Verb.Id == Take && ctx.DirectObject is { } obj && obj != T(Wallet))
                IncrementCarry(ctx);
            if (ctx.Verb.Id == Drop && ctx.DirectObject is { } dropped && dropped != T(Wallet))
                DecrementCarry(ctx);
        });
    }
}
