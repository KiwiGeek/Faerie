namespace Faerie.Model;

/// <summary>
/// Built-in boolean attributes that the standard verbs understand. Authors set these
/// through fluent helpers (e.g. <c>item.Takeable()</c>) rather than touching the flags directly,
/// but the flags are public so custom verbs can query and mutate them without magic strings.
/// </summary>
[Flags]
public enum Attr : long
{
    None = 0,

    /// <summary>Can be picked up and carried.</summary>
    Takeable = 1L << 0,

    /// <summary>Cannot be moved; part of the scenery. Mentioned for examination but never taken.</summary>
    Fixed = 1L << 1,

    /// <summary>Acts as a container that other things can be placed inside.</summary>
    Container = 1L << 2,

    /// <summary>Acts as a supporter that other things can be placed on top of.</summary>
    Supporter = 1L << 3,

    /// <summary>Can be opened and closed.</summary>
    Openable = 1L << 4,

    /// <summary>Currently open (only meaningful with <see cref="Openable"/>).</summary>
    Open = 1L << 5,

    /// <summary>Can be locked and unlocked.</summary>
    Lockable = 1L << 6,

    /// <summary>Currently locked (only meaningful with <see cref="Lockable"/>).</summary>
    Locked = 1L << 7,

    /// <summary>Emits light when active. See <see cref="Lit"/> for current state.</summary>
    LightSource = 1L << 8,

    /// <summary>Currently producing light.</summary>
    Lit = 1L << 9,

    /// <summary>Can be worn.</summary>
    Wearable = 1L << 10,

    /// <summary>Currently being worn.</summary>
    Worn = 1L << 11,

    /// <summary>Can be eaten.</summary>
    Edible = 1L << 12,

    /// <summary>Can be drunk.</summary>
    Drinkable = 1L << 13,

    /// <summary>Represents a living being (NPC, creature). Affects default verb messages.</summary>
    Animate = 1L << 14,

    /// <summary>Has readable text.</summary>
    Readable = 1L << 15,

    /// <summary>Hidden from listings and resolution until revealed.</summary>
    Concealed = 1L << 16,

    /// <summary>A room the player has already visited (set automatically by the engine).</summary>
    Visited = 1L << 17,

    /// <summary>Switchable device (lever, machine). See <see cref="On"/>.</summary>
    Switchable = 1L << 18,

    /// <summary>Currently switched on.</summary>
    On = 1L << 19,

    /// <summary>Treated as plural / uncountable for message generation ("some sand").</summary>
    Plural = 1L << 20,

    /// <summary>Suppresses the automatic "(initial)" description listing in a room.</summary>
    Scenery = 1L << 21,

    /// <summary>
    /// Can be ordered by name when <see cref="Thing.Vendor"/> is present in the current room,
    /// even if the thing itself is still offstage.
    /// </summary>
    Orderable = 1L << 22
}
