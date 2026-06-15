namespace Nixie.Verbs;

/// <summary>
/// Stable identifiers for the built-in verbs. The engine uses a couple of these internally (the
/// movement verb in particular). Game code normally references verb <em>instances</em> returned
/// by the builder rather than these ids.
/// </summary>
public static class StandardVerbIds
{
    public const string Go = "go";
    public const string Look = "look";
    public const string Examine = "examine";
    public const string Inventory = "inventory";
    public const string Take = "take";
    public const string Drop = "drop";
    public const string Open = "open";
    public const string Close = "close";
    public const string Lock = "lock";
    public const string Unlock = "unlock";
    public const string Put = "put";
    public const string Insert = "insert";
    public const string Push = "push";
    public const string Read = "read";
    public const string Wear = "wear";
    public const string Remove = "remove";
    public const string Eat = "eat";
    public const string Drink = "drink";
    public const string PushButton = "switch-on";
    public const string SwitchOff = "switch-off";
    public const string Give = "give";
    public const string Search = "search";
    public const string Wait = "wait";
    public const string Use = "use";

    public const string Help = "help";
    public const string Quit = "quit";
    public const string Save = "save";
    public const string Restore = "restore";
    public const string Score = "score";
}
