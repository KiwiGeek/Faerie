using Faerie.Model;

namespace Faerie.Runtime;

/// <summary>
/// Carry-weight helpers for games that set <see cref="Game.CarryLimit"/> and <see cref="Thing.Size"/>.
/// </summary>
public static class Encumbrance
{
    /// <summary>Default message when <see cref="CanTake"/> fails.</summary>
    public const string DefaultTooHeavyMessage = "Your load is too heavy.";

    /// <summary>Default message for exits that require empty hands.</summary>
    public const string DefaultEmptyHandsMessage = "You have no free hands.";

    /// <summary>Default message for exits that require zero carry load.</summary>
    public const string DefaultNoLoadMessage = "You are carrying too much.";

    /// <summary>True when the player is not holding anything (worn items are allowed).</summary>
    public static bool HandsEmpty(GameContext ctx) => !ctx.State.Inventory.Any();

    /// <summary>True when <see cref="GameState.TotalLoad"/> is zero.</summary>
    public static bool NoLoad(GameContext ctx) => ctx.State.TotalLoad == 0;

    /// <summary>
    /// True when the game has no carry limit, or picking up <paramref name="thing"/> (including
    /// nested contents) would not exceed <see cref="Game.CarryLimit"/>.
    /// </summary>
    public static bool CanTake(GameContext ctx, Thing thing)
    {
        int? limit = ctx.CarryLimit;
        if (limit is null) return true;
        return ctx.State.TotalLoad + ctx.State.LoadOf(thing) <= limit.Value;
    }

    /// <summary>Message to show when <see cref="CanTake"/> is false.</summary>
    public static string TakeBlockedMessage(GameContext ctx, Thing? thing = null) =>
        thing?.OnTakeBlockedMessage ?? DefaultTooHeavyMessage;
}
