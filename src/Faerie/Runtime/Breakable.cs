using Faerie.Model;
using static Faerie.Verbs.VerbText;

namespace Faerie.Runtime;

/// <summary>Helpers for <see cref="Attr.Breakable"/> scenery.</summary>
public static class Breakable
{
    public const string DefaultAlreadyBrokenMessage = "That is already broken.";
    public const string DefaultSuccessMessage = "You break it.";

    /// <summary>
    /// When <paramref name="thing"/> is breakable, applies the broken flag and optional messages.
    /// </summary>
    /// <returns><see langword="null"/> when not breakable; otherwise whether the break succeeded.</returns>
    public static bool? TryBreak(GameContext ctx, Thing thing, out string? message)
    {
        message = null;
        if (!thing.Has(Attr.Breakable)) return null;

        if (thing.Has(Attr.Broken))
        {
            message = thing.BreakAlreadyMessage ?? DefaultAlreadyBrokenMessage;
            return false;
        }

        thing.Set(Attr.Broken);
        thing.OnBreak?.Invoke(ctx);
        message = thing.BreakSuccessMessage ?? $"You break {The(thing)}.";
        return true;
    }
}
