using Faerie.Model;
using static Faerie.Verbs.VerbText;

namespace Faerie.Runtime;

/// <summary>
/// Helpers for <see cref="Thing.PassageDestination"/> — passing carried objects through doors,
/// grates, and similar openings into a linked room.
/// </summary>
public static class Passage
{
    public const string DefaultTooLargeMessage = "It won't fit through.";
    public const string DefaultClosedMessage = "It's closed.";

    /// <summary>
    /// When <paramref name="passage"/> is a passage opening, attempts to move
    /// <paramref name="item"/> to <see cref="Thing.PassageDestination"/>.
    /// </summary>
    /// <returns>
    /// <see langword="null"/> when <paramref name="passage"/> is not a passage;
    /// otherwise <see langword="true"/> on success or <see langword="false"/> on failure
    /// (with <paramref name="message"/> set).
    /// </returns>
    public static bool? TryPass(GameContext ctx, Thing item, Thing passage, out string? message)
    {
        message = null;
        if (passage.PassageDestination is not Room destination) return null;

        if (passage.PassageRequiresOpen && passage.Has(Attr.Openable) && !passage.Has(Attr.Open))
        {
            message = passage.PassageClosedMessage ?? DefaultClosedMessage;
            return false;
        }

        if (passage.PassageMaxSize is int max && item.Size > max)
        {
            message = passage.PassageTooLargeMessage ?? DefaultTooLargeMessage;
            return false;
        }

        ctx.Move(item, Placement.InRoom(destination));
        message = passage.PassageSuccessMessage
            ?? $"The {The(item)} goes through the {The(passage)} into the darkness below.";
        return true;
    }
}
