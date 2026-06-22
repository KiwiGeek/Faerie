namespace Faerie.Runtime;

/// <summary>Passed to <see cref="Game.OnDeath"/> handlers when the player dies.</summary>
public sealed class DeathContext(GameContext context)
{
    public GameContext Context { get; } = context;
    public GameState State => context.State;

    /// <summary>When true, the game continues instead of ending.</summary>
    public bool Revived { get; set; }
}
