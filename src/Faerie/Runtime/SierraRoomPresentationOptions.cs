namespace Faerie.Runtime;

/// <summary>Options for <see cref="RoomPresentations.Sierra"/>.</summary>
public sealed class SierraRoomPresentationOptions
{
    /// <summary>Host input prompt. Default: "What shall I do? ".</summary>
    public string Prompt { get; init; } = "What shall I do? ";

    /// <summary>
    /// Width of the <c>=</c> separator row. Zero uses the terminal width at runtime.
    /// </summary>
    public int SeparatorWidth { get; init; } = 40;
}
