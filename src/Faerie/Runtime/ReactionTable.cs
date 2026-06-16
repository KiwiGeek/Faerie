using Faerie.Model;
using Faerie.Verbs;

namespace Faerie.Runtime;

/// <summary>
/// Holds the "reaction" hooks that let game code intercept verbs without subclassing anything.
/// A reaction is keyed by the (thing, verb) pair so authors write, e.g.,
/// <c>painting.Before(Examine, ctx =&gt; ...)</c> — both referenced by instance, no strings.
/// Global before/after hooks (not tied to a thing) are also stored here.
/// </summary>
public sealed class ReactionTable
{
    private readonly Dictionary<(Thing, Verb), Func<VerbContext, VerbResult>> _before = [];
    private readonly Dictionary<(Thing, Verb), Func<VerbContext, VerbResult>> _after = [];
    private readonly List<Func<VerbContext, VerbResult>> _globalBefore = [];
    private readonly List<Action<VerbContext>> _globalAfter = [];

    public void Before(Thing thing, Verb verb, Func<VerbContext, VerbResult> handler) => _before[(thing, verb)] = handler;
    public void After(Thing thing, Verb verb, Func<VerbContext, VerbResult> handler) => _after[(thing, verb)] = handler;

    public void BeforeAny(Func<VerbContext, VerbResult> handler) => _globalBefore.Add(handler);
    public void AfterAny(Action<VerbContext> handler) => _globalAfter.Add(handler);

    /// <summary>
    /// Runs the "before" reactions for a command. Returns <see cref="VerbResult.Done"/> if any hook
    /// fully handled the command, in which case the default verb behaviour should be skipped.
    /// </summary>
    public VerbResult RunBefore(VerbContext ctx)
    {
        foreach (Func<VerbContext, VerbResult> hook in _globalBefore)
            if (hook(ctx) == VerbResult.Done) return VerbResult.Done;

        if (ctx.DirectObject is { } dobj && _before.TryGetValue((dobj, ctx.Verb), out var h1))
            if (h1(ctx) == VerbResult.Done) return VerbResult.Done;

        if (ctx.IndirectObject is { } iobj && _before.TryGetValue((iobj, ctx.Verb), out var h2))
            if (h2(ctx) == VerbResult.Done) return VerbResult.Done;

        return VerbResult.Pass;
    }

    /// <summary>Runs the "after" reactions once a command's default behaviour has completed.</summary>
    public void RunAfter(VerbContext ctx)
    {
        if (ctx.DirectObject is { } dobj && _after.TryGetValue((dobj, ctx.Verb), out var h1))
            h1(ctx);

        foreach (Action<VerbContext> hook in _globalAfter)
            hook(ctx);
    }
}
