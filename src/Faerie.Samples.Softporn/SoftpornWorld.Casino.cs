using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Softporn;

internal sealed partial class SoftpornWorld
{
    private void PlaySlots(GameContext ctx)
    {
        ReadOnlySpan<char> symbols = ['!', '#', '*', '$', '^'];
        const int symbolCount = 5;

        ctx.Blank();
        ctx.Say("This will cost $100 each time");
        while (true)
        {
            char answer = ctx.PromptKey($"You have ${Money(ctx)}00.  Would you like to play?  (y/n)  ", "YN");
            if (answer != 'Y') break;

            int x1 = 0, x2 = 0, x3 = 0;
            for (int i = 0; i < 30; i++)
            {
                x1 = ctx.Random.Next(symbolCount);
                x2 = ctx.Random.Next(symbolCount);
                x3 = ctx.Random.Next(symbolCount);
                ctx.Delay(30);
                ctx.OverwriteLine($"{symbols[x1]}{symbols[x2],5}{symbols[x3],5}");
            }

            ctx.Out.NewLine();
            if (x1 == x2 && x2 == x3)
            {
                ctx.Say("Triples!!!!!! You win $1500");
                AdjustMoney(ctx, 15);
            }
            else if (x1 == x2 || x2 == x3 || x3 == x1)
            {
                ctx.Say("A pair!  You win $300");
                AdjustMoney(ctx, 3);
            }
            else
            {
                ctx.Say("You lose!");
                Spend(ctx, 1);
            }

            if (Money(ctx) < 1)
            {
                ctx.Say("I'm broke!!! -- that means death!!!!!!!");
                Purgatory(ctx);
                return;
            }
        }

        ctx.Blank();
    }

    private void PlayBlackjack(GameContext ctx)
    {
        string[] cardName =
        [
            "an Ace", "a 2", "a 3", "a 4", "a 5", "a 6", "a 7",
            "an 8", "a 9", "a 10", "a Jack", "a Queen", "a King"
        ];
        const int delay = 400;

        ctx.Blank();
        char again = 'Y';
        while (again == 'Y' && Money(ctx) >= 1)
        {
            int player = 0, dealer = 0;
            int playerSoft = 0, dealerSoft = 0;
            int bet = 0;
            int phase = 1;
            bool roundOver = false;

            while (true)
            {
                ctx.SayInline($"You have ${Money(ctx)}00.  How many dollars would you like to bet? ");
                string dollarString = ctx.PromptLine("").Trim();
                while (dollarString.Contains(' '))
                    dollarString = dollarString.Replace(" ", "");

                string cents;
                if (dollarString.Length > 2)
                {
                    cents = dollarString[^2..];
                    dollarString = dollarString[..^2];
                }
                else
                {
                    cents = dollarString;
                    dollarString = "";
                }

                bool ok = int.TryParse(cents, out bet);
                if (dollarString.Length > 0)
                    ok &= int.TryParse(dollarString, out bet);

                if (!ok || cents != "00" || bet <= 0)
                    ctx.Say("Huh?");
                else if (bet > Money(ctx))
                    ctx.Say("You don't have that much!!!");
                else
                    break;
            }

            while (!roundOver)
            {
                int value = ctx.Random.Next(13) + 1;
                int face = value > 10 ? 10 : value;
                if (value == 1) face = 11;
                string card = cardName[value - 1];

                switch (phase)
                {
                    case 1:
                        player += face;
                        if (value > 9) playerSoft++;
                        if (value == 1) playerSoft++;
                        ctx.Say($"You're dealt {card}");
                        phase = 2;
                        continue;
                    case 2:
                        dealer += face;
                        if (value > 9) dealerSoft++;
                        ctx.Say("The dealer gets a card down");
                        phase = 3;
                        continue;
                    case 3:
                        dealer += face;
                        if (value == 1) dealerSoft++;
                        if (value > 9) dealerSoft++;
                        ctx.Say($"The dealer gets {card}");
                        phase = 4;
                        goto CheckPlayer;
                    case 4:
                        player += face;
                        if (value == 1) playerSoft++;
                        if (value > 9) playerSoft++;
                        ctx.Say($"You get {card}");
                        goto CheckPlayer;
                    case 5:
                        dealer += face;
                        if (value == 1) dealerSoft++;
                        if (value > 9) dealerSoft++;
                        ctx.Say($"The dealer gets {card}");
                        goto DealerTurn;
                }

                continue;

            CheckPlayer:
                while (player > 21 && playerSoft > 0) { playerSoft--; player -= 10; }
                ctx.Delay(delay);
                ctx.Say($"Your total is {player}.");
                if (player > 21)
                {
                    ctx.Delay(delay);
                    ctx.Say("Busted!");
                    Spend(ctx, bet);
                    roundOver = true;
                }
                else if (playerSoft == 2 && player == 21)
                {
                    ctx.Delay(delay);
                    ctx.Say("You've got a ***BLACKJACK***");
                    AdjustMoney(ctx, bet + bet);
                    roundOver = true;
                }
                else if (dealerSoft == 2 && dealer == 21)
                {
                    ctx.Delay(delay);
                    ctx.Say("The dealer has a ***BLACKJACK***");
                    Spend(ctx, bet);
                    roundOver = true;
                }
                else
                {
                    char hit = ctx.PromptKey("Would you like a hit?  (y/n)  ", "YN");
                    if (hit == 'N')
                        goto DealerTurn;
                    phase = 4;
                }

                continue;

            DealerTurn:
                while (dealer > 21 && dealerSoft > 0) { dealerSoft--; dealer -= 10; }
                ctx.Delay(delay);
                ctx.Say($"The dealer has {dealer}");
                if (dealer < 17)
                {
                    phase = 5;
                    continue;
                }

                if (dealer > 21 || player > dealer)
                {
                    ctx.Delay(delay);
                    ctx.Say("You win!!");
                    AdjustMoney(ctx, bet);
                }
                else if (player < dealer)
                {
                    ctx.Delay(delay);
                    ctx.Say("You lose!");
                    Spend(ctx, bet);
                }
                else
                {
                    ctx.Delay(delay);
                    ctx.Say("Tie!");
                }

                roundOver = true;
            }

            if (Money(ctx) < 1)
            {
                ctx.Say("You're out of money!!!  So long!!!!!!!!!!");
                Purgatory(ctx);
                return;
            }

            again = ctx.PromptKey("Play again?? (y/n)  ", "YN");
        }

        ctx.Blank();
    }
}
