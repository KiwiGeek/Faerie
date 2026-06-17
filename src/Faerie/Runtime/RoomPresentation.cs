namespace Faerie.Runtime;

/// <summary>
/// Optional hooks that control how rooms are described and redrawn. When null on <see cref="Game"/>,
/// the engine uses built-in Infocom-style presentation.
/// </summary>
public sealed class RoomPresentation
{
    /// <summary>
    /// Prints room prose for enter, re-enter, look, and lighting changes. The engine decides
    /// <em>when</em> to call; the hook decides <em>what</em> to print.
    /// </summary>
    public Action<RoomDescribeContext>? DescribeRoom { get; init; }

    /// <summary>
    /// Redraws compact per-turn room status (Sierra banners, HUDs, etc.). Called after game start
    /// and at the end of each turn.
    /// </summary>
    public Action<GameContext>? RefreshRoomDisplay { get; init; }

    /// <summary>
    /// Optional input prompt for the host (e.g. Sierra's "What shall I do?"). When set on the
    /// presentation, <see cref="Building.GameBuilder.WithRoomPresentation"/> copies it to the game.
    /// </summary>
    public string? InputPrompt { get; init; }
}
