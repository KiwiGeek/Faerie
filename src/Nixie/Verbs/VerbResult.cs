namespace Nixie.Verbs;

/// <summary>
/// The outcome of a verb handler or reaction hook. <see cref="Done"/> stops further processing
/// for the turn; <see cref="Pass"/> lets the default behaviour (or other handlers) run.
/// </summary>
public enum VerbResult
{
    /// <summary>The action was fully handled; stop processing this command.</summary>
    Done,

    /// <summary>This handler declined to act; fall through to default behaviour.</summary>
    Pass
}
