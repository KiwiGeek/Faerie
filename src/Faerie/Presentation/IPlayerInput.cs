namespace Faerie.Presentation;

/// <summary>
/// Blocking player input during verb execution (mid-turn prompts). Hosts provide an implementation
/// for interactive play; headless runners feed subsequent script lines.
/// </summary>
public interface IPlayerInput
{
    /// <summary>Blocks until the player submits a line of text.</summary>
    string ReadLine();

    /// <summary>Blocks until the player presses a key in <paramref name="validKeys"/> (case-insensitive).</summary>
    char ReadKey(ReadOnlySpan<char> validKeys);
}
