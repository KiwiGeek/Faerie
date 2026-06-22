namespace Faerie.Samples.Zork;

/// <summary>
/// Catalog of Zork I behaviors that are simplified because Faerie
/// lacks the underlying systems.
/// Full analysis: AGENTS.md in this project (section "Zork I port: engine gaps").
/// Puzzle code references these with ENGINE-LIMIT comments.
/// </summary>
internal static class ZorkSimplifications
{
    /// <summary>AGENTS.md §2 — IMPLEMENTED: board/disembark/launch, river landings, beach carry limits.
    /// See ZorkWorld.Boat.cs.</summary>
    internal const string BoatAndRiver = nameof(BoatAndRiver);

    /// <summary>AGENTS.md §3 — IMPLEMENTED: melee combat, score-based fight strength, weapon weaknesses,
    /// thrown weapons, thief disengage. See ZorkWorld.DefineCombat / ZorkWorld.CombatFidelity.cs.</summary>
    internal const string Combat = nameof(Combat);

    /// <summary>AGENTS.md §4 — IMPLEMENTED: Loud Room output/input filters; ECHO silences room.
    /// Turn-end scramble via OnTurn. See ZorkWorld.DefineLoudRoom / ZorkWorld.Dam.cs.</summary>
    internal const string LoudRoom = nameof(LoudRoom);

    /// <summary>AGENTS.md §5 — IMPLEMENTED: I-THIEF roam, bag booty, sacred rooms, treasure-room defense,
    /// engrossed state, single-item rob tables, lantern snuff. See ZorkWorld.Thief.cs.</summary>
    internal const string Thief = nameof(Thief);

    /// <summary>AGENTS.md §6 — IMPLEMENTED: yellow/brown gate flag, bolt toggle, 8-turn drain/fill,
    /// blue-button flood, red lights, loud-room integration. See ZorkWorld.Dam.cs.</summary>
    internal const string Dam = nameof(Dam);

    /// <summary>AGENTS.md §7 — IMPLEMENTED: open-flame hazard on enter, each turn, and lighting.
    /// See ZorkWorld.GasRoom.cs.</summary>
    internal const string GasRoom = nameof(GasRoom);

    /// <summary>AGENTS.md §8 — IMPLEMENTED: rub teleport/swap, break, bad-luck flag.
    /// See ZorkWorld.Mirrors.cs.</summary>
    internal const string Mirrors = nameof(Mirrors);

    /// <summary>AGENTS.md §9 — IMPLEMENTED: bell drops hot, candles drop and go out, relight, read prayer.
    /// See ZorkWorld.Hades.cs. Pour-water-on-bell recovery included.</summary>
    internal const string Hades = nameof(Hades);

    /// <summary>AGENTS.md §10 — IMPLEMENTED: pass-through PUT, unlock/lock sides, leaves reveal/drop.
    /// See ZorkWorld.Grating.cs.</summary>
    internal const string Grating = nameof(Grating);

    /// <summary>AGENTS.md §12 — IMPLEMENTED: dynamic trophy-case scoring, touch bonuses, place/task awards.
    /// See ZorkWorld.Scoring.cs.</summary>
    internal const string Scoring = nameof(Scoring);

    /// <summary>AGENTS.md §13 — IMPLEMENTED: revival with treasure scatter to dark rooms.
    /// See ZorkWorld.Death.cs.</summary>
    internal const string Death = nameof(Death);

    /// <summary>AGENTS.md §14 — IMPLEMENTED: drop from Up a Tree breaks egg (`Thing.OnDrop`);
    /// open egg or wind canary on Forest Path for bauble. See ZorkWorld.DefineEggAndCanary.</summary>
    internal const string Egg = nameof(Egg);

    /// <summary>AGENTS.md §15 — IMPLEMENTED: proximity glow daemon (none/faint/bright). See ZorkWorld.DefineSwordGlow.</summary>
    internal const string Sword = nameof(Sword);

    /// <summary>AGENTS.md §16 — IMPLEMENTED: lunch/water wrath stages, I-CYCLOPS daemon, ODYSSEUS verb,
    /// sword dodge. Uses <see cref="CreatureMood"/> and <see cref="Fluid"/>. See ZorkWorld.Cyclops.cs.</summary>
    internal const string Cyclops = nameof(Cyclops);

    /// <summary>AGENTS.md §17 — IMPLEMENTED: bat steals treasure to shaft room; garlic repels.
    /// See ZorkWorld.Bat.cs.</summary>
    internal const string Bat = nameof(Bat);

    /// <summary>AGENTS.md §18 — IMPLEMENTED: four digs reveal scarab; fifth without scarab taken collapses.
    /// See ZorkWorld.Puzzles.DefineSandAndScarab.</summary>
    internal const string Sand = nameof(Sand);

    /// <summary>AGENTS.md §19 — IMPLEMENTED: board/launch/land/disembark, throw, brandish, smell, listen,
    /// count, shake, fill, answer, plug, lower/raise basket. See ZorkWorld.Verbs.cs, ZorkWorld.Basket.cs, ZorkWorld.Boat.cs.</summary>
    internal const string Verbs = nameof(Verbs);

    /// <summary>AGENTS.md §20 — IMPLEMENTED: gallery gnome sets magic flag; chimney flag on studio-up climb;
    /// cyclops east wall via ODYSSEUS. See ZorkWorld.MagicPassage.cs, ZorkWorld.Cyclops.cs.</summary>
    internal const string MagicPassage = nameof(MagicPassage);
}
