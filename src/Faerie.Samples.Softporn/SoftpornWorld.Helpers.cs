using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Softporn;

internal sealed partial class SoftpornWorld
{
    private const int MaxCarried = 7;

    private int Money(GameContext ctx) => ctx.Get(_money);

    private void AdjustMoney(GameContext ctx, int deltaHundreds)
    {
        ctx.Set(_money, Money(ctx) + deltaHundreds);
        ctx.RefreshStatusBars();
    }

    private void Spend(GameContext ctx, int hundreds) => AdjustMoney(ctx, -hundreds);

    private bool HasMoney(GameContext ctx, int hundreds) => Money(ctx) >= hundreds;

    private bool CarryingWallet(GameContext ctx) => ctx.Carrying(T(Wallet));

    private int CarryCount(GameContext ctx) => ctx.Get(_carryCount);

    private void IncrementCarry(GameContext ctx) => ctx.Set(_carryCount, CarryCount(ctx) + 1);

    private void DecrementCarry(GameContext ctx) => ctx.Set(_carryCount, Math.Max(0, CarryCount(ctx) - 1));

    private bool IsOffstage(GameContext ctx, Thing thing) =>
        !ctx.Carrying(thing) && !ctx.Wearing(thing) && ctx.RoomOf(thing) is null;

    private void RevealHere(GameContext ctx, Thing thing)
    {
        if (IsOffstage(ctx, thing))
        {
            ctx.Say("I see something!!!");
            ctx.PlaceHere(thing);
        }
    }

    // A blank line followed by a block of text — the game's standard way of delivering a long reply.
    private static void SayLong(GameContext ctx, string text)
    {
        ctx.Blank();
        ctx.Say(text);
    }

    private void CantDoThat(GameContext ctx)
    {
        string[] lines =
        [
            "Huh?", "Ummm......huh?", "You're nuts!", "You can't be serious!!",
            "Not bloody likely!!", "I don't know how to.", "An interesting idea....", "I can't do that."
        ];
        ctx.Blank();
        ctx.Say(lines[ctx.Random.Next(lines.Length)]);
    }

    private void FindMeOne(GameContext ctx)
    {
        string[] lines = ["Find me one!!", "I don't see it here!", "I can't find it here!", "You have to find it first!"];
        ctx.Blank();
        ctx.Say(lines[ctx.Random.Next(lines.Length)]);
    }

    private void Purgatory(GameContext ctx)
    {
        Thread.Sleep(700);
        int door = 0;
        while (true)
        {
            if (door == 0)
                SayLong(ctx,
"""
Welcome to Purgatory!! Here at this crossroad you have three options:
Before you are three doors. Each will bring you to a different place -
A - To Hell (where the game ends)
B - Back to life, unharmed
C - You stay here and must choose again
The doors are randomly different each time!!
""");
            else
            {
                ctx.Say("You're still here!");
                ctx.Blank();
            }

            char choice = ctx.PromptKey("Choose your door: 1, 2 or 3??  ", "123");
            door = (ctx.Random.Next(3) + (choice - '0')) % 3;
            if (door == 1)
            {
                ctx.Lose("Welcome to Hell.");
                return;
            }

            if (door != 2)
                return;
        }
    }

    private void FallingDown(GameContext ctx, bool jumped)
    {
        for (int i = 0; i < 50; i++)
            ctx.Say("Aaaaaeeeeeiiiiiiii!!!!!!!!");
        Thread.Sleep(300);
        ctx.Say("Splaaatttttt!!!!!");
        if (!jumped)
        {
            Thread.Sleep(500);
            ctx.Blank();
            ctx.Say("I should have used safety rope!!!!!!!!");
        }

        Purgatory(ctx);
    }

    internal void ShowHelp(GameContext ctx)
    {
        ctx.Blank();
        ctx.Blank();
        ctx.Say(
"""
The object of SOFTPORN ADVENTURE is to find -- and seduce -- three different
girls. They have very different personalities, so tricks that work on one
girl usually won't work on another girl.
The game has three different areas -- Disco, Casino and Bar. You start off
in the Bar with $1000. You'll need more money than that, so you'll have to
make more money during the game.
You give your puppet commands like 'Go north', 'Buy beer' etc. A phrase like
'Look at the table' is equivalent to 'Look table'. Some commands can be abbre-
viated, like 'N' for 'Go north', 'L' for 'Look', 'I' for 'Inventory' etc.
To see how well you're doing, type 'Score'. To save and restore a game posi-
tion, type 'Save' and 'Restore', followed (optionally) by up to 3 characters,
specifying different saved game positions.
If you're stuck, try looking at everything in sight, object by object. This
will usually help you find missing objects and/or clues etc.
The 'Help' command will repeat this message.
Several commands can be entered on the same line, but must then be separated
by commas or periods.
 ========= ******** GOOD LUCK !!! ******** ========
""");
        ctx.Blank();
        ctx.PromptKey("                    Press  <SPACE>  to continue  ", " ");
        ctx.Blank();
    }

    private void Ok(GameContext ctx) => ctx.Say("OK");

    private void WatchTv(GameContext ctx)
    {
        while (true)
        {
            char ch = ctx.PromptKey("Which channel? (1-9) ", "123456789");
            ctx.Set(_tvChannel, ch - '0');
            SayLong(ctx, TvChannelText(ch - '0'));
            ch = ctx.PromptKey("Change the channel?  (y/n) ", "YN");
            if (ch == 'N') break;
        }
    }

    private static string TvChannelText(int channel) => channel switch
    {
        1 =>
"""
A masked man runs across the screen. Jumping up he lands on his horse and
yells 'Hi-ho plutonium!!!!!' He rides off into a green sky.....
Nothing like a good old western to pass the time.
""",
        2 =>
"""
It's 'The price is fright!!!!'
'And now for out favorite host ..... Haunty Male!!!!!!!!!!'
Haunty jumps up the stage - he asks 'And who's our first lucky contestant?'
The announcer points out a lady ... the crowd screams in ecstacy as she's
dragged to the stage...........
""",
        3 =>
"""
Captain Jerk looks at this door from which behind the noise is coming.
Throwing open the door - his face turns a deep red!!!!!!!!!
He says: 'Scotty! What are you doing??'
Scotty replies: 'Byt Captain!?!? My girl and I - we're engaged!!!!'
Jerk commands: 'Well, then disengage!'
... as the starship thrusted forward ... penetrating deeper into space ....
""",
        4 =>
"""
The News!! Today the prime rate was raised once again ... to 257%! This
does not come near the record set in 1996 - when it broke the 1000% mark....
The birth rate has taken a dramatic fall ... this is due to the increased
usage of computers as sexual partners.. However ... rapes of innocent people
are on the increase! And who is the rapist?? Computerized banking machines
lead the list ... followed by home computers ....
""",
        5 =>
"""
Mr Rodjerk jumps up with his big sneakers and says in his cherry voice:
'Guess what, boys and girls???? Today we're going to learn about suckers!!
Susie .. see the lolly-pop??? Can you stick it in your mouth??? That's
right! That's a nice lolly-pop ... nice and hardright?!?!?!?!?......'
""",
        6 =>
"""
Cable TV!!!!!!!!
They're showing the kinkiest X-rated movies!!!!
This one's titled 'Deep Nostril'. The Pimp likes this one!!!!
He's engrossed in the action he sees!!! Seems distracted.............
""",
        7 =>
"""
It's Happy Daze!!!!!
Richie turns to Gonzy and says 'But you always had it made with the girls....
What's your secret???'
The Gonz says 'Aayyyyyy .. I didn't get my name for nuthin!'
reaching into his pocket he pulls out a funny looking cigarette.........
""",
        8 =>
"""
Mrs Smith and mrs Jones are comparing detergents ........ see this bluse?
We're making it this dirty to see who's works better...
(A dog is thrown onto the blouse. In his excitement he deficates all over it)
Do you think yours will work, mrs Smith?
(The camera pans to mrs Smith. She throws up.)
Mrs Jones???
(A shot shows her taking the dog and........... )
""",
        _ =>
"""
It's the super bowl!!!!
The center snaps the ball!!
The quarterback fades back!!
It's a bomb!!!!!
The ball sails through the air ... the receiver runs to get it.........
It explodes in his hands!!!!! What a bomb!!!!!
Tell me, Howard, have you ever seen this before???
""",
    };

    private void BuyRubber(GameContext ctx)
    {
        ctx.Set(_rubberLubricated, "non-lubricated");
        ctx.Set(_rubberRibbed, "non-ribbed");
        ctx.Blank();
        ctx.Say("The man leans over the counter and whispers:");
        ctx.Set(_rubberColor, ctx.PromptLine("What color? ").ToLowerInvariant());
        ctx.Set(_rubberFlavor, ctx.PromptLine("And for a flavor? ").ToLowerInvariant());
        char lub = ctx.PromptKey("Lubricated or not? (y/n) ", "YN");
        if (lub == 'Y')
            ctx.Set(_rubberLubricated, "lubricated");
        char rib = ctx.PromptKey("Ribbed? (y/n) ", "YN");
        if (rib == 'Y')
            ctx.Set(_rubberRibbed, "ribbed");
        ctx.Say($"He yells -- This pervert just bought a {ctx.Get(_rubberColor)}, ");
        ctx.Say($"{ctx.Get(_rubberFlavor)}-flavored, {ctx.Get(_rubberLubricated)}, {ctx.Get(_rubberRibbed)} rubber!!!!");
        ctx.Say("A lady walks by and looks at me in disgust!!!!");
        ctx.Blank();
    }

    private bool TryGiveWineToBum(GameContext ctx)
    {
        if (!ctx.LocatedIn(T(Bum), ctx.CurrentRoom))
            return false;

        if (IsOffstage(ctx, T(Knife)))
        {
            BumTellsStory(ctx);
            ctx.PlaceHere(T(Knife));
        }
        else
            ctx.Say("The bum mutters 'That stuff made me puke!! Get out of here!!!'");

        ctx.Remove(T(Wine));
        return true;
    }

    private void BumTellsStory(GameContext ctx)
    {
        ctx.Blank();
        ctx.Say("He looks at me and starts to speak:");
        ctx.Delay(400);
        SayLong(ctx,
"""
Well, my son ... here's my story.
I came here many years ago - and my goals were the same as yours ... but this
adventure was too much for me!
Here's a gift ... carry it with you at all times!!!! There's some kinky girls
in this town!! And you never know when you may need to use this to defend
yourself!!!! After all you may get in a program bug ......
""");
        ctx.Blank();
        ctx.Delay(300);
        for (int i = 0; i <= 128; i++)
        {
            ctx.Say(new string(' ', i) + "Like I did!!");
            if (ctx.LivePresentation)
            {
                ctx.Delay(0);
                ctx.Delay(20);
            }
        }
        for (int i = 0; i < 5; i++)
            ctx.Blank();
        ctx.Delay(500);
        ctx.Say("He throws up and gives me back the bottle of wine.");
        ctx.Blank();
    }

    private static string NormalizePhone(string? text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? ""
            : text.Replace("555", "", StringComparison.OrdinalIgnoreCase)
            .Replace("-", "")
            .Trim();
    }

    private static string TitleCaseFirst(string s)
    {
        return string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s[1..].ToLowerInvariant() : "");
    }
}
