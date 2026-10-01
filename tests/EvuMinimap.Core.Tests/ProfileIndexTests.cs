using EvuMinimap.Core;
using Xunit;

namespace EvuMinimap.Core.Tests;

public sealed class ProfileIndexTests
{
    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(5, 1, 1)]
    [InlineData(1, -1, 5)]
    [InlineData(3, -1, 2)]
    public void Cycle_WrapsBetweenOneAndFive(int active, int direction, int expected)
    {
        Assert.Equal(expected, ProfileIndex.Cycle(active, direction));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 5)]
    [InlineData(8, 5)]
    public void Clamp_StaysInsideTheProfileCount(int index, int expected)
    {
        Assert.Equal(expected, ProfileIndex.Clamp(index));
    }
}
