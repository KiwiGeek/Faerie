namespace Faerie.Runtime;

/// <summary>How the engine presents the current room after each turn.</summary>
public enum RoomBannerStyle
{
    /// <summary>Infocom-style heading, prose, and inline contents/exits (the default).</summary>
    None,

    /// <summary>
    /// Sierra / Hi-Res Adventure layout: long prose once per visit, then each turn a short title,
    /// comma-separated items and exits, and a row of separator characters.
    /// </summary>
    Sierra
}
