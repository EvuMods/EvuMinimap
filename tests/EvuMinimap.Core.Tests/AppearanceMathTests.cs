using System;
using EvuMinimap.Core;
using Xunit;

namespace EvuMinimap.Core.Tests;

public sealed class AppearanceMathTests
{
    [Theory]
    [InlineData(0f, 0f)]
    [InlineData(1f, 1f)]
    [InlineData(0.4f, 0.4f)]
    [InlineData(-0.2f, 0f)]
    [InlineData(1.5f, 1f)]
    public void ClampAlpha_StaysInsideZeroToOne(float input, float expected)
    {
        Assert.Equal(expected, AppearanceMath.ClampAlpha(input));
    }

    [Fact]
    public void ClampAlpha_TreatsNaNAsZero()
    {
        Assert.Equal(0f, AppearanceMath.ClampAlpha(float.NaN));
    }

    [Theory]
    [InlineData(0f, 0f)]
    [InlineData(1f, 1f)]
    [InlineData(0.25f, 0.25f)]
    [InlineData(-1f, 0f)]
    [InlineData(2f, 1f)]
    public void ClampCornerRadius_StaysInsideZeroToOne(float input, float expected)
    {
        Assert.Equal(expected, AppearanceMath.ClampCornerRadius(input));
    }

    [Fact]
    public void ClampCornerRadius_TreatsNaNAsZero()
    {
        Assert.Equal(0f, AppearanceMath.ClampCornerRadius(float.NaN));
    }

    [Theory]
    [InlineData(0.5f, 0.5f)]
    [InlineData(1f, 1f)]
    [InlineData(2f, 2f)]
    [InlineData(0.1f, 0.5f)]
    [InlineData(4f, 2f)]
    public void ClampAspect_StaysInsideTheRange(float input, float expected)
    {
        Assert.Equal(expected, AppearanceMath.ClampAspect(input));
    }

    [Fact]
    public void ClampAspect_TreatsNaNAsTheMinimum()
    {
        Assert.Equal(AppearanceMath.MinAspect, AppearanceMath.ClampAspect(float.NaN));
    }
}
