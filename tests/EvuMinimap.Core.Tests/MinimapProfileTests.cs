using System;
using EvuMinimap.Core;
using Xunit;

namespace EvuMinimap.Core.Tests;

public sealed class MinimapProfileTests
{
    [Fact]
    public void Constructor_ClampsScaleAlphaCornerAndAspect()
    {
        var high = new MinimapProfile(
            MapAnchor.Center,
            1f,
            2f,
            9f,
            false,
            MapShape.Rectangle,
            4f,
            3f,
            8f,
            false);
        Assert.Equal(ScaleMath.Max, high.Scale);
        Assert.Equal(1f, high.Alpha);
        Assert.Equal(1f, high.CornerRadius);
        Assert.Equal(AppearanceMath.MaxAspect, high.Aspect);
        Assert.False(high.RepositionBuffs);
        Assert.False(high.RepositionShipHud);
        Assert.Equal(MapShape.Rectangle, high.Shape);

        var low = new MinimapProfile(
            MapAnchor.Center,
            0f,
            0f,
            0.1f,
            true,
            MapShape.Oval,
            float.NaN,
            -1f,
            0.1f);
        Assert.Equal(ScaleMath.Min, low.Scale);
        Assert.Equal(0f, low.Alpha);
        Assert.Equal(0f, low.CornerRadius);
        Assert.Equal(AppearanceMath.MinAspect, low.Aspect);
    }
}
