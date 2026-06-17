namespace Faerie.Model;

/// <summary>
/// Result of evaluating whether an <see cref="Exit"/> may be traversed.
/// Use <see cref="Open"/> or <c>null</c> to allow passage; use <see cref="Block"/> or a
/// non-null string (via implicit conversion) to block with a player-facing message.
/// </summary>
public readonly struct ExitGate
{
    private readonly string? _blocked;

    public bool CanPass => _blocked is null;

    /// <summary>When <see cref="CanPass"/> is false, the message to show the player.</summary>
    public string? BlockedMessage => _blocked;

    private ExitGate(string? blocked) => _blocked = blocked;

    public static ExitGate Open => new(null);

    public static ExitGate Block(string message) => new(message);

    /// <summary>true = open; false = blocked with a generic message (pair with <c>blocked:</c> on Connect).</summary>
    public static implicit operator ExitGate(bool pass) =>
        pass ? Open : Block("You can't go that way.");

    /// <summary>null = open; otherwise blocked with that message.</summary>
    public static implicit operator ExitGate(string? blocked) =>
        blocked is null ? Open : Block(blocked);
}
