namespace Faerie.Samples.Softporn.Interpreter;

public static class SoftpornHelpers
{
    public static char Locase(char c) => c switch
    {
        >= 'A' and <= 'Z' => (char)(c + 0x20),
        _ => c
    };

    public static void Newlines(SoftpornContext ctx, int lines)
    {
        for (var i = 0; i < lines; i++)
            ctx.Out.WriteLine();
    }

    public static void ICantGoThatWay(SoftpornContext ctx) =>
        ctx.Messages.WriteMessage("I can't go that way!");

    public static void CantDoThat(SoftpornContext ctx)
    {
        var messg = ctx.Rng.Next(8) + 1;
        ctx.Out.WriteLine(messg switch
        {
            1 => "Huh?",
            2 => "Ummm......huh?",
            3 => "You're nuts!",
            4 => "You can't be serious!!",
            5 => "Not bloody likely!!",
            6 => "I don't know how to.",
            7 => "An interesting idea....",
            _ => "I can't do that."
        });
    }

    public static void Huh(SoftpornContext ctx) =>
        ctx.Messages.WriteMessage("Huh?");

    public static void ICantDoThat(SoftpornContext ctx)
    {
        ctx.Out.WriteLine();
        CantDoThat(ctx);
    }

    public static void IDontKnowThatWord(SoftpornContext ctx) =>
        ctx.Messages.WriteMessage("I don't know that word!");

    public static void FindMeOne(SoftpornContext ctx)
    {
        var messg = ctx.Rng.Next(4) + 1;
        ctx.Out.WriteLine();
        ctx.Out.WriteLine(messg switch
        {
            1 => "Find me one!!",
            2 => "I don't see it here!",
            3 => "I can't find it here!",
            _ => "You have to find it first!"
        });
    }

    public static void IDontHaveIt(SoftpornContext ctx) =>
        ctx.Messages.WriteMessage("I don't have it!!");

    public static void IAlreadyHaveIt(SoftpornContext ctx) =>
        ctx.Messages.WriteMessage("I already have it!!");

    public static void ISeeNothingSpecial(SoftpornContext ctx) =>
        ctx.Messages.WriteMessage("I see nothing special");

    public static void ISeeSomething(SoftpornContext ctx, Objects obj, string messg)
    {
        if (ctx.State.ObjectPlace[(int)obj] == Places.Nowhere)
        {
            ctx.Messages.WriteMessage("I see something!!!");
            ctx.State.ObjectPlace[(int)obj] = ctx.State.YourPlace;
        }
        else if (messg == "")
            ISeeNothingSpecial(ctx);
        else
            ctx.Messages.WriteMessage(messg);
    }

    public static void NotYetButMaybeLater(SoftpornContext ctx) =>
        ctx.Messages.WriteMessage("Not yet but maybe later!");

    public static void SorryNoMoney(SoftpornContext ctx) =>
        ctx.Messages.WriteMessage("Sorry -- no money!!");

    public static bool IsHere(SoftpornContext ctx, Objects obj) =>
        ctx.State.ObjectPlace[(int)obj] == ctx.State.YourPlace;

    public static bool IsCarried(SoftpornContext ctx, Objects obj) =>
        ctx.State.ObjectPlace[(int)obj] == Places.YouHaveIt;

    public static bool YouAreIn(SoftpornContext ctx, Places place) =>
        ctx.State.YourPlace == place;

    public static void WaitForSpace(SoftpornContext ctx)
    {
        ctx.Out.Write("                    Press  <SPACE>  to continue  ");
        while (true)
        {
            var c = ctx.Io.ReadKey("", " ");
            if (c == ' ')
                break;
            ctx.Io.WriteBell();
        }
    }

    public static void GiveHelp(SoftpornContext ctx)
    {
        ctx.Out.WriteLine();
        ctx.Out.WriteLine();
        for (var message = 70; message <= 72; message++)
            ctx.Messages.WriteLongMessg(message);
        ctx.Out.WriteLine();
        WaitForSpace(ctx);
        ctx.Out.WriteLine();
    }

    public static void LookGraffiti(SoftpornContext ctx)
    {
        ctx.Out.WriteLine();
        for (var message = 59; message <= 62; message++)
            ctx.Messages.WriteLongMessg(message);
        WaitForSpace(ctx);
        ctx.Out.WriteLine();
    }

    public static void Purgatory(SoftpornContext ctx)
    {
        Thread.Sleep(700);
        var door = 0;
        while (true)
        {
            if (door == 0)
                ctx.Messages.WriteLongMessage(65);
            else
            {
                ctx.Out.WriteLine();
                ctx.Out.WriteLine("You're still here!");
                ctx.Out.WriteLine();
            }

            ctx.Out.Write("Choose your door: 1, 2 or 3??  ");
            var c = ctx.Io.ReadKey("", "123");
            var choice = c - '0';
            door = (ctx.Rng.Next(3) + choice) % 3;
            ctx.State.GameEnded = door == 1;
            if (door != 2)
                break;
        }
    }

    public static void BumTellsStory(SoftpornContext ctx)
    {
        ctx.Out.WriteLine();
        ctx.Out.WriteLine("He looks at me and starts to speak:");
        Thread.Sleep(400);
        ctx.Messages.WriteLongMessage(39);
        ctx.Out.WriteLine();
        Thread.Sleep(300);
        for (var i = 0; i <= 128; i++)
        {
            ctx.Out.Write(new string(' ', i));
            ctx.Out.WriteLine("Like I did!!");
        }
        for (var i = 0; i < 5; i++)
            ctx.Out.WriteLine();
        Thread.Sleep(500);
        ctx.Out.WriteLine("He throws up and gives me back the bottle of wine.");
        ctx.Out.WriteLine();
    }

    public static void WatchTv(SoftpornContext ctx)
    {
        while (true)
        {
            ctx.Out.Write("Which channel? (1-9) ");
            var ch = ctx.Io.ReadKey("", "123456789");
            ctx.State.TvChannel = ch - '0';
            ctx.Messages.WriteLongMessage(40 + ctx.State.TvChannel);
            ctx.Out.WriteLine();
            ctx.Out.Write("Change the channel?  (y/n) ");
            ch = ctx.Io.ReadKey("", "YN");
            if (ch == 'N')
                break;
        }
    }

    public static void WineInTaxi(SoftpornContext ctx)
    {
        ctx.Messages.WriteLongMessage(58);
        Thread.Sleep(500);
        ctx.Out.WriteLine();
        ctx.Out.Write("What shall I do? ");
        Thread.Sleep(1000);
        ctx.Out.WriteLine();
        ctx.Out.WriteLine();
        ctx.Out.WriteLine("The idiot cab driver backs over me and kills me!!!!!!");
        Purgatory(ctx);
    }

    public static void StabSomeone(SoftpornContext ctx)
    {
        ctx.Out.WriteLine();
        ctx.Out.WriteLine("OK - warmonger!");
        Thread.Sleep(1000);
        ctx.Out.WriteLine("Parry!!");
        Thread.Sleep(500);
        ctx.Out.WriteLine("Thrust!!!");
        Thread.Sleep(1000);
        ctx.Out.WriteLine("I just got myself!!");
        Purgatory(ctx);
    }

    public static void FallingDown(SoftpornContext ctx)
    {
        for (var i = 0; i < 50; i++)
            ctx.Out.WriteLine("Aaaaaeeeeeiiiiiiii!!!!!!!!");
        Thread.Sleep(300);
        ctx.Out.WriteLine("Splaaatttttt!!!!!");
        if (ctx.State.Verb != Verbs.Jump)
        {
            Thread.Sleep(500);
            ctx.Out.WriteLine();
            ctx.Out.WriteLine("I should have used safety rope!!!!!!!!");
        }
        Purgatory(ctx);
    }

    public static void PlaySlot(SoftpornContext ctx)
    {
        ReadOnlySpan<char> slot = ['!', '#', '*', '$', '^'];
        const int slotFigs = 5;

        ctx.Out.WriteLine();
        ctx.Out.WriteLine("This will cost $100 each time");
        char answer = 'Y';
        while (true)
        {
            ctx.Out.Write($"You have ${ctx.State.Money}00.  Would you like to play?  (y/n)  ");
            answer = ctx.Io.ReadKey("", "YN");
            if (answer != 'Y')
                break;

            int x1 = 0, x2 = 0, x3 = 0;
            for (var i = 0; i < 30; i++)
            {
                x1 = ctx.Rng.Next(slotFigs);
                x2 = ctx.Rng.Next(slotFigs);
                x3 = ctx.Rng.Next(slotFigs);
                Thread.Sleep(30);
                ctx.Out.Write($"\r{slot[x1]}{slot[x2],5}{slot[x3],5}");
            }
            ctx.Out.WriteLine();
            if (x1 == x2 && x2 == x3)
            {
                ctx.Out.WriteLine("Triples!!!!!! You win $1500");
                ctx.State.Money += 15;
            }
            else if (x1 == x2 || x2 == x3 || x3 == x1)
            {
                ctx.Out.WriteLine("A pair!  You win $300");
                ctx.State.Money += 3;
            }
            else
            {
                ctx.Out.WriteLine("You lose!");
                ctx.State.Money -= 1;
            }

            if (ctx.State.Money < 1)
                break;
        }

        ctx.Out.WriteLine();
        if (ctx.State.Money < 1)
        {
            ctx.Out.WriteLine("I'm broke!!! -- that means death!!!!!!!");
            Purgatory(ctx);
        }
    }

    public static void Play21(SoftpornContext ctx)
    {
        string[] cardName =
        [
            "an Ace", "a 2", "a 3", "a 4", "a 5", "a 6", "a 7",
            "an 8", "a 9", "a 10", "a Jack", "a Queen", "a King"
        ];
        const int delay21 = 400;

        ctx.Out.WriteLine();
        char answer = 'Y';
        while (answer == 'Y' && ctx.State.Money >= 1)
        {
            var mi = 0;
            var md = 0;
            var yd = 0;
            var ym = 0;
            var ad = 0;
            var am = 0;
            var dollars = 0;
            int z = 0, y = 0, ac = 0;
            string card = "";
            var gameOver = false;

            while (true)
            {
                var answerOk = false;
                ctx.Out.Write($"You have ${ctx.State.Money}00.  How many dollars would you like to bet? ");
                var dollarString = ctx.Io.ReadLine("").Trim();
                while (dollarString.Contains(' '))
                    dollarString = dollarString.Replace(" ", "");

                string doll00;
                if (dollarString.Length > 2)
                {
                    doll00 = dollarString[^2..];
                    dollarString = dollarString[..^2];
                }
                else
                {
                    doll00 = dollarString;
                    dollarString = "";
                }

                var code = 0;
                if (!int.TryParse(doll00, out dollars))
                    code = 1;
                if (code == 0 && dollarString != "" && !int.TryParse(dollarString, out dollars))
                    code = 1;

                if (code != 0)
                    ctx.Out.WriteLine("Huh?");
                else if (doll00 != "00")
                    ctx.Out.WriteLine("$100 increments only!!");
                else if (dollars <= 0)
                    ctx.Out.WriteLine("Huh?");
                else if (dollars > ctx.State.Money)
                    ctx.Out.WriteLine("You don't have that much!!!");
                else
                    answerOk = true;

                if (answerOk)
                    break;
            }

            var a = 1;
            gameOver = false;

            while (!gameOver)
            {
                z = ctx.Rng.Next(13) + 1;
                y = 0;
                ac = 0;
                card = cardName[z - 1];
                if (z > 10) z = 10;
                if (z == 1) z = 11;
                if (z > 9) y = 1;
                if (z == 11) ac = 1;

                Thread.Sleep(delay21);
                switch (a)
                {
                    case 1:
                    case 3:
                        mi += z;
                        ctx.Out.WriteLine($"You're dealt {card}");
                        ym += y;
                        am += ac;
                        a++;
                        break;
                    case 2:
                        md += z;
                        ctx.Out.WriteLine("The dealer gets a card down");
                        yd += y;
                        ad += ac;
                        a++;
                        break;
                    case 4:
                        md += z;
                        ctx.Out.WriteLine($"The dealer gets {card}");
                        a = 5;
                        ad += ac;
                        yd += y;
                        goto CheckHit;
                    case 5:
                        mi += z;
                        ctx.Out.WriteLine($"You get {card}");
                        am += ac;
                        goto CheckHit;
                    case 6:
                        md += z;
                        ctx.Out.WriteLine($"The dealer gets {card}");
                        ad += ac;
                        goto DealerCheck;
                }

                continue;

            CheckHit:
                if (mi > 21 && am > 0)
                {
                    am--;
                    mi -= 10;
                }
                Thread.Sleep(delay21);
                ctx.Out.WriteLine($"Your total is {mi}.");
                if (mi > 21)
                {
                    Thread.Sleep(delay21);
                    ctx.Out.WriteLine("Busted!");
                    ctx.State.Money -= dollars;
                    gameOver = true;
                }
                else if (ym == 2 && mi == 21)
                {
                    Thread.Sleep(delay21);
                    ctx.Out.WriteLine("You've got a ***BLACKJACK***");
                    ctx.State.Money += dollars + dollars;
                    gameOver = true;
                }
                else if (yd == 2 && md == 21)
                {
                    Thread.Sleep(delay21);
                    ctx.Out.WriteLine("The dealer has a ***BLACKJACK***");
                    ctx.State.Money -= dollars;
                    gameOver = true;
                }
                else
                {
                    Thread.Sleep(delay21);
                    ctx.Out.Write("Would you like a hit?  (y/n)  ");
                    answer = ctx.Io.ReadKey("", "YN");
                    if (answer == 'N')
                        goto DealerCheck;
                }
                continue;

            DealerCheck:
                if (md > 21 && ad > 0)
                {
                    ad--;
                    md -= 10;
                }
                Thread.Sleep(delay21);
                ctx.Out.WriteLine($"The dealer has {md}");
                if (md < 17)
                    a = 6;
                else if (md > 21 || mi > md)
                {
                    Thread.Sleep(delay21);
                    ctx.Out.WriteLine("You win!!");
                    ctx.State.Money += dollars;
                    gameOver = true;
                }
                else if (mi < md)
                {
                    Thread.Sleep(delay21);
                    ctx.Out.WriteLine("You lose!");
                    ctx.State.Money -= dollars;
                    gameOver = true;
                }
                else
                {
                    Thread.Sleep(delay21);
                    ctx.Out.WriteLine("Tie!");
                    gameOver = true;
                }
            }

            if (ctx.State.Money < 1)
            {
                ctx.Out.WriteLine("You're out of money!!!  So long!!!!!!!!!!");
                Purgatory(ctx);
                return;
            }

            ctx.Out.Write("Play again?? (y/n)  ");
            answer = ctx.Io.ReadKey("", "YN");
        }
    }

    public static void BuyRubber(SoftpornContext ctx)
    {
        var s = ctx.State;
        s.RubberLubricated = "non-lubricated";
        s.RubberRibbed = "non-ribbed";
        ctx.Out.WriteLine();
        ctx.Out.WriteLine("The man leans over the counter and whispers:");
        ctx.Out.Write("What color? ");
        s.RubberColor = ctx.Io.ReadLine("").ToLowerInvariant();
        ctx.Out.Write("And for a flavor? ");
        s.RubberFlavor = ctx.Io.ReadLine("").ToLowerInvariant();
        ctx.Out.Write("Lubricated or not? (y/n) ");
        var answer = ctx.Io.ReadKey("", "YN");
        if (answer == 'Y')
            s.RubberLubricated = s.RubberLubricated[4..];
        ctx.Out.Write("Ribbed? (y/n) ");
        answer = ctx.Io.ReadKey("", "YN");
        if (answer == 'Y')
            s.RubberRibbed = s.RubberRibbed[4..];
        ctx.Out.WriteLine($"He yells -- This pervert just bought a {s.RubberColor}, ");
        ctx.Out.WriteLine($"{s.RubberFlavor}-flavored, {s.RubberLubricated}, {s.RubberRibbed} rubber!!!!");
        ctx.Out.WriteLine("A lady walks by and looks at me in disgust!!!!");
        ctx.Out.WriteLine();
    }

    public static void Ok(SoftpornContext ctx) =>
        ctx.Messages.WriteMessage("OK");

    public static void Open(SoftpornContext ctx, Func<bool> getOpen, Action<bool> setOpen)
    {
        if (getOpen())
            ctx.Messages.WriteMessage("It's already open!!");
        else
        {
            Ok(ctx);
            setOpen(true);
        }
    }

    public static void Close(SoftpornContext ctx, Func<bool> getOpen, Action<bool> setOpen)
    {
        if (!getOpen())
            ctx.Messages.WriteMessage("It's already closed!!");
        else
        {
            Ok(ctx);
            setOpen(false);
        }
    }

    public static void InitNewGame(SoftpornContext ctx, bool cheat = false)
    {
        ctx.Out.WriteLine();
        ctx.Out.WriteLine("Welcome to SOFTPORN ADVENTURE!!");
        Newlines(ctx, 2);
        ctx.Out.Write("Do you need instructions? (y/n) ");
        var yesno = ctx.Io.ReadKey("", "YN");
        if (yesno == 'Y')
            GiveHelp(ctx);
        else
            Newlines(ctx, 2);

        ctx.LineFromKbd = "";
        ctx.State.ResetFromOrig(cheat);
    }

    public static void LookAround(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (!s.PlaceVisited[(int)s.YourPlace])
            ctx.Messages.WriteLongMessage((int)s.YourPlace + 1);

        if (s.YourPlace == Places.PPntpch && s.Called5550439)
        {
            if (!s.TelephoneAnswered && ctx.Rng.Next(4) == 2)
                s.TelephoneRinging = true;
            if (s.TelephoneRinging)
                ctx.Messages.WriteMessage("The telephone rings");
        }

        s.PlaceVisited[(int)s.YourPlace] = true;

        ctx.Out.WriteLine(SoftpornConstants.PlaceName[(int)s.YourPlace]);

        ctx.Out.Write("Items in sight are: ");
        var hpos = 23;
        var objcount = 0;
        for (var obj = (int)SoftpornConstants.FirstObject; obj <= (int)SoftpornConstants.LastObject - 1; obj++)
        {
            if (!IsHere(ctx, (Objects)obj))
                continue;

            if (objcount > 0)
            {
                ctx.Out.Write(", ");
                hpos += 2;
            }
            objcount++;
            var namelen = SoftpornConstants.ObjectName[obj].Length;
            if (hpos + 3 + namelen > 80)
            {
                ctx.Out.WriteLine();
                ctx.Out.Write("                      ");
                hpos = 23;
            }
            ctx.Out.Write(SoftpornConstants.ObjectName[obj]);
            hpos += namelen;
        }

        if (objcount == 0)
            ctx.Out.WriteLine("Nothing interesting.");
        else
            ctx.Out.WriteLine();

        ctx.Out.Write("Other areas are: ");
        var exitcount = 0;
        for (var exit = 0; exit <= (int)Directions.Down; exit++)
        {
            if (s.Path[(int)s.YourPlace, exit] != Places.Nowhere)
                exitcount++;
        }
        var exits = exitcount;
        if (exits == 0)
            ctx.Out.Write("By magic!");
        else
        {
            exitcount = exits;
            for (var exit = 0; exit <= (int)Directions.Down; exit++)
            {
                if (s.Path[(int)s.YourPlace, exit] == Places.Nowhere)
                    continue;
                if (exitcount < exits)
                    ctx.Out.Write(", ");
                else if (exits > 1)
                    ctx.Out.Write(" and ");
                exitcount--;
                ctx.Out.Write(SoftpornConstants.DirectionName[exit]);
            }
        }

        ctx.Out.WriteLine();
        ctx.Out.WriteLine(new string('=', 79));
    }

    public static void TakeInventory(SoftpornContext ctx)
    {
        var s = ctx.State;
        ctx.Out.WriteLine();
        ctx.Out.WriteLine("I'm carrying: ");
        var objcount = 0;
        for (var obj = (int)SoftpornConstants.FirstObject; obj <= (int)SoftpornConstants.LastObject - 1; obj++)
        {
            if (!IsCarried(ctx, (Objects)obj))
                continue;
            objcount++;
            ctx.Out.WriteLine(SoftpornConstants.ObjectName[obj]);
        }
        if (objcount == 0)
            ctx.Out.WriteLine("Nothing");
        else
            ctx.Out.WriteLine();
    }

    public static void RemoveLeadingSpaces(ref string str)
    {
        while (str.Length > 0 && str[0] == ' ')
            str = str[1..];
    }

    public static void RemoveLeadingSpacesAndPeriods(ref string str)
    {
        while (str.Length > 0 && (str[0] == ' ' || str[0] == '.'))
            str = str[1..];
    }

    public static void RemoveTrailingSpaces(ref string str)
    {
        while (str.Length > 0 && str[^1] == ' ')
            str = str[..^1];
    }

    public static void RemoveMultipleSpaces(ref string str)
    {
        while (str.Contains(' '))
            str = str.Replace(" ", "");
    }

    public static void ExpandAbbreviations(ref string str)
    {
        var str4 = (str + " ")[..Math.Min(4, str.Length + 1)].ToUpperInvariant();
        if (str4 == "INVE")
            str = "I";

        if (str.Length == 1)
        {
            var ch1 = char.ToUpperInvariant(str[0]);
            str = ch1 switch
            {
                'I' => "TAKE INVE",
                'N' => "GO NORT",
                'S' => "GO SOUT",
                'E' => "GO EAST",
                'W' => "GO WEST",
                'U' => "GO UP",
                'D' => "GO DOWN",
                'L' => "LOOK",
                _ => str
            };
        }
    }

    public static void AddDefiniteArticleTo(ref string fullNoun)
    {
        if (fullNoun.Length > 0 && SoftpornConstants.Vowels.Contains(fullNoun[0]))
            fullNoun = " an " + fullNoun;
        else
            fullNoun = " a " + fullNoun;
    }

    public static void SplitUpInVerbAndNoun(string command, out string verb, out string noun,
        out string fullVerb, out string fullNoun)
    {
        const string spaces = "    ";
        verb = spaces;
        noun = spaces;
        fullVerb = "";
        fullNoun = "";

        RemoveLeadingSpaces(ref command);

        for (var i = 0; i < 2; i++)
        {
            string fullWord;
            while (true)
            {
                var p = command.IndexOf(' ');
                if (p < 0)
                {
                    fullWord = command;
                    command = "";
                }
                else
                {
                    fullWord = command[..p];
                    command = command[(p + 1)..];
                    RemoveLeadingSpaces(ref command);
                }

                var word = PadWord(fullWord);
                var glueWord = SoftpornConstants.GlueWords.Any(g => word == g);
                if (!glueWord)
                    break;
            }

            if (i == 0)
            {
                verb = PadWord(fullWord);
                fullVerb = fullWord;
            }
            else
            {
                noun = PadWord(fullWord);
                fullNoun = fullWord;
            }
        }

        foreach (var (orig, repl) in SoftpornConstants.SynVerb)
        {
            if (verb == orig)
                verb = repl;
        }

        foreach (var (orig, repl) in SoftpornConstants.SynNoun)
        {
            if (noun == orig)
                noun = repl;
        }
    }

    public static string PadWord(string fullWord)
    {
        var word = fullWord.ToUpperInvariant();
        if (word.Length >= SoftpornConstants.WordNameLength)
            return word[..SoftpornConstants.WordNameLength];
        return word.PadRight(SoftpornConstants.WordNameLength);
    }

    public static void ReadAndParseCommand(SoftpornContext ctx, out string verb, out string noun,
        out string fullVerb, out string fullNoun)
    {
        verb = "    ";
        noun = "    ";
        fullVerb = "";
        fullNoun = "";

        while (true)
        {
            if (ctx.LineFromKbd == "")
            {
                while (true)
                {
                    LookAround(ctx);
                    while (true)
                    {
                        ctx.Out.WriteLine();
                        ctx.Out.Write("What shall I do? ");
                        ctx.LineFromKbd = ctx.Io.ReadLine("");
                        if (ctx.LineFromKbd == "")
                            ctx.Messages.WriteMessage("Beg pardon?");
                        else
                            break;
                    }

                    while (ctx.LineFromKbd.Contains('-'))
                        ctx.LineFromKbd = ctx.LineFromKbd.Replace("-", "");

                    var chars = ctx.LineFromKbd.ToCharArray();
                    for (var i = 0; i < chars.Length; i++)
                    {
                        if (chars[i] is '!' or '?' or ',')
                            chars[i] = '.';
                        else if (chars[i] is >= '!' and <= '-' or '/' or >= ':' and <= '?')
                            chars[i] = ' ';
                    }
                    ctx.LineFromKbd = new string(chars);

                    var kbdLine = ctx.LineFromKbd;
                    RemoveLeadingSpacesAndPeriods(ref kbdLine);
                    RemoveTrailingSpaces(ref kbdLine);
                    RemoveMultipleSpaces(ref kbdLine);
                    ctx.LineFromKbd = kbdLine;
                    if (ctx.LineFromKbd == " ")
                        ctx.LineFromKbd = "";
                    if (ctx.LineFromKbd == "")
                        ICantDoThat(ctx);
                    else
                        break;
                }
            }

            string command;
            var dotPos = ctx.LineFromKbd.IndexOf('.');
            if (dotPos >= 0)
            {
                command = ctx.LineFromKbd[..dotPos];
                ctx.LineFromKbd = ctx.LineFromKbd[(dotPos + 1)..];
                var remainder = ctx.LineFromKbd;
                RemoveLeadingSpacesAndPeriods(ref remainder);
                ctx.LineFromKbd = remainder;
                RemoveTrailingSpaces(ref command);
            }
            else
            {
                command = ctx.LineFromKbd;
                ctx.LineFromKbd = "";
            }

            ExpandAbbreviations(ref command);

            while (command.Contains("555"))
                command = command.Replace("555", "");

            SplitUpInVerbAndNoun(command, out verb, out noun, out fullVerb, out fullNoun);

            var commandOk = verb != "    ";

            if (noun == "LADY")
            {
                ctx.Messages.WriteMessage("That's no Lady!!! That's my sister!!!!");
                commandOk = false;
            }
            else if (verb == "SAY ")
            {
                ctx.Messages.WriteMessage("OK");
                if (command.Length > 4)
                    ctx.Out.WriteLine(command[4..]);
                commandOk = false;
            }
            else if (verb is "TKAE" or "TAEK")
            {
                ctx.Messages.WriteMessage("Learn to spell, idiot!!!");
                commandOk = false;
            }

            if (commandOk)
                break;
        }
    }
}
