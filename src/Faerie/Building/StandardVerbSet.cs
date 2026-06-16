using Faerie.Verbs;

namespace Faerie.Building;

/// <summary>
/// Named handles to the built-in verbs. Once a verb module is installed the corresponding handle
/// is populated, letting game code attach reactions by instance (e.g.
/// <c>builder.On(painting).Before(builder.Verbs.Examine, ...)</c>) without any magic strings.
/// A handle is null until its module has been added.
/// </summary>
public sealed class StandardVerbSet
{
    // Movement
    public Verb? Go { get; internal set; }

    // Observation
    public Verb? Look { get; internal set; }
    public Verb? Examine { get; internal set; }
    public Verb? Search { get; internal set; }
    public Verb? Inventory { get; internal set; }

    // Manipulation
    public Verb? Take { get; internal set; }
    public Verb? Drop { get; internal set; }
    public Verb? Open { get; internal set; }
    public Verb? Close { get; internal set; }
    public Verb? Lock { get; internal set; }
    public Verb? Unlock { get; internal set; }
    public Verb? Put { get; internal set; }
    public Verb? Push { get; internal set; }
    public Verb? Read { get; internal set; }
    public Verb? Wear { get; internal set; }
    public Verb? TakeOff { get; internal set; }
    public Verb? Eat { get; internal set; }
    public Verb? Drink { get; internal set; }
    public Verb? SwitchOn { get; internal set; }
    public Verb? SwitchOff { get; internal set; }
    public Verb? Give { get; internal set; }
    public Verb? Use { get; internal set; }
    public Verb? Wait { get; internal set; }

    // Meta
    public Verb? Help { get; internal set; }
    public Verb? Quit { get; internal set; }
    public Verb? Save { get; internal set; }
    public Verb? Restore { get; internal set; }
    public Verb? Score { get; internal set; }
}
