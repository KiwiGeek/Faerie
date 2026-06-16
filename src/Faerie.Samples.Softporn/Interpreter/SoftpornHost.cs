namespace Faerie.Samples.Softporn.Interpreter;

public sealed class SoftpornHost
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly IEnumerable<string>? _scriptLines;
    private readonly bool _cheat;

    public SoftpornHost(TextReader input, TextWriter output, IEnumerable<string>? scriptLines = null, bool cheat = false)
    {
        _input = input;
        _output = output;
        _scriptLines = scriptLines;
        _cheat = cheat;
    }

    public void Run()
    {
        var state = new GameState();
        var io = new SoftpornIo(_input, _output, _scriptLines);
        var messages = new SoftpornMessages(_output);
        var ctx = new SoftpornContext(state, io, messages);

        char yesno;
        do
        {
            SoftpornHelpers.InitNewGame(ctx, _cheat);
            state.GameEnded = false;

            while (!state.GameEnded)
            {
                ApplyAreaUpdates(ctx);

                SoftpornHelpers.ReadAndParseCommand(ctx, out var verbnam, out var objnam,
                    out var fullVerb, out var fullNoun);
                ctx.FullVerb = fullVerb;
                ctx.FullNoun = fullNoun;

                state.VerbNam = verbnam;
                state.ObjNam = objnam;

                ctx.VerbOnly = objnam[0] == ' ';

                state.Verb = SoftpornConstants.FirstVerb;
                while (verbnam != SoftpornConstants.VerbName[(int)state.Verb] &&
                       state.Verb < SoftpornConstants.LastVerb)
                    state.Verb++;

                ctx.NoVerb = state.Verb == Verbs.NoVerb;

                state.Noun = SoftpornConstants.FirstObject;
                while (objnam != SoftpornConstants.ObjName[(int)state.Noun] &&
                       state.Noun < SoftpornConstants.LastObject)
                    state.Noun++;

                ctx.NoObject = state.Noun == Objects.NoObject;

                state.Direction = SoftpornConstants.FirstDirection;
                while (objnam != SoftpornConstants.DirName[(int)state.Direction] &&
                       state.Direction <= SoftpornConstants.LastDirection)
                    state.Direction++;

                ctx.NoDirection = state.Direction == Directions.NoDirection;

                if (state.Noun == Objects.Sign)
                {
                    if (state.YourPlace == Places.DEntrnc)
                        state.Noun = Objects.DoorWest;
                    if (state.YourPlace == Places.PKitchn)
                        state.Noun = Objects.Sink;
                }

                if (ctx.NoVerb)
                    messages.WriteMessage("I don't know how to " + ctx.FullVerb + " something!");
                else if (ctx.VerbOnly && !SoftpornConstants.StandAloneVerbs.Contains(state.Verb))
                    messages.WriteMessage("Gimme a noun!!");
                else if (!ctx.VerbOnly && ctx.NoObject && ctx.NoDirection &&
                         !SoftpornConstants.SpecialVerbs.Contains(state.Verb))
                {
                    var nounPhrase = ctx.FullNoun;
                    SoftpornHelpers.AddDefiniteArticleTo(ref nounPhrase);
                    messages.WriteMessage("I don't know how to " + ctx.FullVerb + nounPhrase + "!");
                }
                else
                    SoftpornVerbs.Dispatch(ctx);
            }

            _output.WriteLine();
            _output.WriteLine($"You scored '{state.Score}' out of a possible '3'");
            SoftpornHelpers.Newlines(ctx, 2);
            _output.Write("Thanks for playing. Would you like to play again? ");
            yesno = io.ReadKey("", "YN");
        } while (yesno != 'N');

        messages.WriteMessage("Good-Bye!!");
        _output.WriteLine();
    }

    internal static void ApplyAreaUpdates(SoftpornContext ctx)
    {
        var s = ctx.State;

        if (SoftpornConstants.BarArea.Contains(s.YourPlace))
        {
            s.ObjectPlace[(int)Objects.Sign] = Places.BStreet;
            s.ObjectPlace[(int)Objects.Button] = Places.BBar;
        }
        else if (SoftpornConstants.CasinoArea.Contains(s.YourPlace))
        {
            s.ObjectPlace[(int)Objects.Sign] = Places.CStreet;
            s.ObjectPlace[(int)Objects.Button] = Places.CHtdesk;
            s.ObjectPlace[(int)Objects.Elevator] = Places.CHtdesk;
        }
        else if (SoftpornConstants.DiscoArea.Contains(s.YourPlace))
        {
            s.ObjectPlace[(int)Objects.Sign] = Places.DStreet;
            s.ObjectPlace[(int)Objects.Telephone] = Places.DTelbth;
        }
        else if (SoftpornConstants.PenthouseArea.Contains(s.YourPlace))
        {
            s.ObjectPlace[(int)Objects.Button] = Places.PPntfoy;
            s.ObjectPlace[(int)Objects.Elevator] = Places.PPntfoy;
            s.ObjectPlace[(int)Objects.Telephone] = Places.PPntpch;
        }

        if (s.YourPlace != Places.BBar)
            s.Path[(int)Places.BBar, (int)Directions.East] = Places.Nowhere;

        if (s.YourPlace != Places.DEntrnc)
        {
            s.Path[(int)Places.DEntrnc, (int)Directions.West] = Places.Nowhere;
            s.DoorWOpen = false;
        }

        if (!SoftpornHelpers.IsHere(ctx, Objects.Stool))
            s.StoolClimbed = false;

        if (s.RubberWorn && SoftpornConstants.PublicPlaces.Contains(s.YourPlace))
        {
            if (ctx.Rng.Next(8) == 5)
            {
                ctx.Messages.WriteMessage("A passerby kills me for wearing my kinky rubber in public!");
                SoftpornHelpers.Purgatory(ctx);
            }
        }
    }
}
