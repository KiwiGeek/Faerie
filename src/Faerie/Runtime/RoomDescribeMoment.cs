namespace Faerie.Runtime;

/// <summary>When the engine asks the game to describe or refresh the current room.</summary>
public enum RoomDescribeMoment
{
    /// <summary>First time the player enters this room.</summary>
    FirstEnter,

    /// <summary>Returning to a room the player has visited before.</summary>
    ReEnter,

    /// <summary>The player typed LOOK (or equivalent).</summary>
    Look,

    /// <summary>A light source was lit and the room is now visible.</summary>
    LightingChanged
}
