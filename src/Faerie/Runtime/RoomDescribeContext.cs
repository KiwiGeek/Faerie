using Faerie.Model;
using Faerie.Presentation;

namespace Faerie.Runtime;

/// <summary>Context passed to <see cref="RoomPresentation.DescribeRoom"/>.</summary>
public sealed record RoomDescribeContext(GameContext Context, Room Room, RoomDescribeMoment Moment)
{
    public GameState State => Context.State;
    public OutputWriter Out => Context.Out;
}
