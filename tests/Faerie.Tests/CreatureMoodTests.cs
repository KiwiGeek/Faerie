using Xunit;
using static Faerie.Runtime.CreatureMood;

namespace Faerie.Tests;

public sealed class CreatureMoodTests
{
    [Fact]
    public void Escalate_MovesAwayFromZero()
    {
        Assert.Equal(1, Escalate(0));
        Assert.Equal(2, Escalate(1));
        Assert.Equal(-2, Escalate(-1));
        Assert.Equal(-3, Escalate(-2));
    }

    [Fact]
    public void IsLethal_UsesAbsoluteValue()
    {
        Assert.False(IsLethal(5, 6));
        Assert.True(IsLethal(6, 6));
        Assert.True(IsLethal(-6, 6));
    }

    [Fact]
    public void ThirstyAfterMeal_SetsNegativeMood()
    {
        Assert.Equal(-1, ThirstyAfterMeal(0));
        Assert.Equal(-3, ThirstyAfterMeal(3));
        Assert.Equal(-3, ThirstyAfterMeal(-3));
    }
}
