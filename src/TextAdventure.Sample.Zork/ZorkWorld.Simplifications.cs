namespace TextAdventure.Sample.Zork;

/// <summary>
/// Catalog of Zork I behaviors that are simplified because Nixie
/// lacks the underlying systems.
/// Full analysis: AGENTS.md in this project (section "Zork I port: engine gaps").
/// Puzzle code references these with ENGINE-LIMIT comments.
/// </summary>
internal static class ZorkSimplifications
{
    /// <summary>AGENTS.md §1 — No carry weight; coffin/empty-hands rules partial only.</summary>
    internal const string Encumbrance = nameof(Encumbrance);

    /// <summary>AGENTS.md §2 — Inflate/deflate only; no river boarding or water-room travel.</summary>
    internal const string BoatAndRiver = nameof(BoatAndRiver);

    /// <summary>AGENTS.md §3 — IMPLEMENTED: turn-by-turn melee (hit/miss/wound/knockout/disarm/kill),
    /// player health, unconscious recovery and wake-on-leave. See ZorkWorld.DefineCombat/CombatRound.</summary>
    internal const string Combat = nameof(Combat);

    /// <summary>AGENTS.md §4 — IMPLEMENTED: the Loud Room echoes all output via the engine's output-filter
    /// seam (GameBuilder.FilterOutput); ECHO silences it. See ZorkWorld.DefineLoudRoom.</summary>
    internal const string LoudRoom = nameof(LoudRoom);

    /// <summary>AGENTS.md §5 — Random steal daemon; no thief bag, roaming, or sacred rooms.</summary>
    internal const string Thief = nameof(Thief);

    /// <summary>AGENTS.md §6 — Bolt instantly drains reservoir; no bubble/buttons or flood timer.</summary>
    internal const string Dam = nameof(Dam);

    /// <summary>AGENTS.md §7 — Gas-room explosion with open flame not implemented.</summary>
    internal const string GasRoom = nameof(GasRoom);

    /// <summary>AGENTS.md §8 — Mirror rooms connected; no break/swap behavior.</summary>
    internal const string Mirrors = nameof(Mirrors);

    /// <summary>AGENTS.md §9 — Hades opens with bell+book+candles in inventory; no ritual sequence.</summary>
    internal const string Hades = nameof(Hades);

    /// <summary>AGENTS.md §10 — Grating is a gated exit; no push-through from clearing.</summary>
    internal const string Grating = nameof(Grating);

    /// <summary>AGENTS.md §11 — GratingRoom.IsDark cleared manually when grating opens.</summary>
    internal const string GratingLight = nameof(GratingLight);

    /// <summary>AGENTS.md §12 — One-time trophy bits; no death penalty or case removal penalty.</summary>
    internal const string Scoring = nameof(Scoring);

    /// <summary>AGENTS.md §13 — No treasure scatter on death.</summary>
    internal const string Death = nameof(Death);

    /// <summary>AGENTS.md §14 — Open egg releases canary; no break-on-drop from tree.</summary>
    internal const string Egg = nameof(Egg);

    /// <summary>AGENTS.md §15 — IMPLEMENTED: proximity glow daemon (none/faint/bright). See ZorkWorld.DefineSwordGlow.</summary>
    internal const string Sword = nameof(Sword);

    /// <summary>AGENTS.md §16 — Cyclops sleep simplified; no mood counter or water quantity.</summary>
    internal const string Cyclops = nameof(Cyclops);

    /// <summary>AGENTS.md §17 — Bat room instant death without garlic; no steal loop.</summary>
    internal const string Bat = nameof(Bat);

    /// <summary>AGENTS.md §18 — Four digs reveal scarab; sand collapse death omitted.</summary>
    internal const string Sand = nameof(Sand);

    /// <summary>AGENTS.md §19 — Many Infocom verbs absent; minimal custom verb set added.</summary>
    internal const string Verbs = nameof(Verbs);

    /// <summary>AGENTS.md §20 — Magic/chimney flags on first-enter; no gnome or cyclops wall break.</summary>
    internal const string MagicPassage = nameof(MagicPassage);
}
