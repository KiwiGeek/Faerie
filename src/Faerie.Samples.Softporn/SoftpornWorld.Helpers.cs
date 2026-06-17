using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Softporn;

internal sealed partial class SoftpornWorld
{
    private const int MaxCarried = 7;

    private int Money(GameContext ctx) => ctx.Get(_money);

    private void Spend(GameContext ctx, int hundreds)
    {
        ctx.Set(_money, Money(ctx) - hundreds);
    }

    private bool HasMoney(GameContext ctx, int hundreds) => Money(ctx) >= hundreds;

    private bool CarryingWallet(GameContext ctx) => ctx.Carrying(T(SoftpornIds.Wallet));

    private int CarryCount(GameContext ctx) => ctx.Get(_carryCount);

    private void IncrementCarry(GameContext ctx) => ctx.Set(_carryCount, CarryCount(ctx) + 1);

    private void DecrementCarry(GameContext ctx) => ctx.Set(_carryCount, Math.Max(0, CarryCount(ctx) - 1));

    private bool IsOffstage(GameContext ctx, Thing thing) =>
        !ctx.Carrying(thing) && !ctx.Wearing(thing) && ctx.RoomOf(thing) is null;

    private void RevealHere(GameContext ctx, string thingId)
    {
        Thing thing = T(thingId);
        if (IsOffstage(ctx, thing))
        {
            ctx.Say("I see something!!!");
            ctx.PlaceHere(thing);
        }
    }

    private void SayMsg(GameContext ctx, int n) => SoftpornMessages.Say(ctx, n);

    private void SayLong(GameContext ctx, int n) => SoftpornMessages.SayLong(ctx, n);

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
                SayLong(ctx, 65);
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
        for (int m = 70; m <= 72; m++)
            ctx.Say(SoftpornMessages.Text(m));
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
            SayLong(ctx, 40 + (ch - '0'));
            ch = ctx.PromptKey("Change the channel?  (y/n) ", "YN");
            if (ch == 'N') break;
        }
    }

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

    private void BumTellsStory(GameContext ctx)
    {
        ctx.Blank();
        ctx.Say("He looks at me and starts to speak:");
        Thread.Sleep(400);
        SayLong(ctx, 39);
        ctx.Blank();
        Thread.Sleep(300);
        for (int i = 0; i <= 128; i++)
        {
            ctx.Say(new string(' ', i) + "Like I did!!");
        }
        for (int i = 0; i < 5; i++)
            ctx.Blank();
        Thread.Sleep(500);
        ctx.Say("He throws up and gives me back the bottle of wine.");
        ctx.Blank();
    }

    private static string NormalizePhone(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        return text.Replace("555", "", StringComparison.OrdinalIgnoreCase)
            .Replace("-", "")
            .Trim();
    }

    private static string TitleCaseFirst(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s[1..].ToLowerInvariant() : "");
    }
}
