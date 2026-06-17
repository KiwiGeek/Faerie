namespace Faerie.Runtime;

/// <summary>
/// A one-shot action scheduled to run when <see cref="GameState.TurnCount"/> reaches
/// <see cref="DueAtTurn"/>. Registered via
/// <see cref="Faerie.Building.GameBuilder.ScheduleIn(int, System.Action{Faerie.Runtime.GameContext}, System.Func{Faerie.Runtime.GameContext, bool}?)"/> or
/// <see cref="Faerie.Runtime.GameContext.ScheduleIn(int, System.Action{Faerie.Runtime.GameContext}, System.Func{Faerie.Runtime.GameContext, bool}?)"/>.
/// </summary>
public sealed class ScheduledTimer
{
    public string? Name { get; init; }
    public int DueAtTurn { get; set; }
    public required Action<GameContext> Action { get; init; }
    public Func<GameContext, bool>? Condition { get; init; }
    public bool Fired { get; set; }
    public bool Cancelled { get; set; }

    public void Tick(GameContext ctx)
    {
        if (Fired || Cancelled) return;
        if (ctx.State.TurnCount < DueAtTurn) return;
        if (Condition is not null && !Condition(ctx)) return;

        Action(ctx);
        Fired = true;
    }
}
