using EvuMinimap.Core;
using Xunit;

namespace EvuMinimap.Core.Tests;

public sealed class ScaleMathTests
{
    [Theory]
    [InlineData(1f, 1f)]
    [InlineData(0.5f, 0.5f)]
    [InlineData(5f, 5f)]
    [InlineData(0.1f, 0.5f)]
    [InlineData(8f, 5f)]
    public void Clamp_StaysInsideTheRange(float input, float expected)
    {
        Assert.Equal(expected, ScaleMath.Clamp(input));
    }

    [Fact]
    public void Step_MovesOneTick()
    {
        Assert.Equal(1.25f, ScaleMath.Step(1f, 0.25f, 1));
        Assert.Equal(0.75f, ScaleMath.Step(1f, 0.25f, -1));
    }

    [Fact]
    public void Step_StopsAtTheEnds()
    {
        Assert.Equal(5f, ScaleMath.Step(5f, 0.25f, 1));
        Assert.Equal(0.5f, ScaleMath.Step(0.5f, 0.25f, -1));
    }
}
