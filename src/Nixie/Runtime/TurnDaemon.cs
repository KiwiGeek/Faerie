namespace Nixie.Runtime;

/// <summary>
/// A piece of logic that runs as time passes: a roaming ghost, a candle burning down, a timed
/// trap. Daemons are registered through the builder (EveryTurn, AfterTurns, When) and evaluated by
/// the engine at the start of each turn.
/// </summary>
public sealed class TurnDaemon
{
    public required Action<GameContext> Action { get; init; }

    /// <summary>Runs every turn while active.</summary>
    public bool EveryTurn { get; init; }

    /// <summary>If set, runs once when <see cref="GameState.TurnCount"/> reaches this value.</summary>
    public int? AtTurn { get; init; }

    /// <summary>Optional gate; the action only fires when this returns true.</summary>
    public Func<GameContext, bool>? Condition { get; init; }

    /// <summary>True once a one-shot daemon has fired.</summary>
    public bool Fired { get; set; }

    public void Tick(GameContext ctx)
    {
        if (Fired && AtTurn is not null) return;

        bool due = EveryTurn || (AtTurn is { } t && ctx.State.TurnCount >= t);
        if (!due) return;

        if (Condition is not null && !Condition(ctx)) return;

        Action(ctx);
        if (AtTurn is not null) Fired = true;
    }
}
