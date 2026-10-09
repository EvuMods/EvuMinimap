using EvuMinimap.Core;
using Xunit;

namespace EvuMinimap.Core.Tests;

public sealed class ShapeKeyTests
{
    [Theory]
    [InlineData(0.5f)]
    [InlineData(1f)]
    [InlineData(1.25f)]
    [InlineData(2f)]
    public void Oval_DoesNotShareAKeyWithNoneOrASharpRectangle(float aspect)
    {
        var oval = ShapeKey.For(MapShape.Oval, 0f, aspect);
        var rectangle = ShapeKey.For(MapShape.Rectangle, 0f, aspect);
        var none = ShapeKey.For(MapShape.None, 0f, aspect);
        Assert.NotEqual(oval, rectangle);
        Assert.NotEqual(oval, none);
        Assert.NotEqual(rectangle, none);
    }

    [Fact]
    public void RectangleRadiiAndAspects_StayDistinct()
    {
        var sharp = ShapeKey.For(MapShape.Rectangle, 0f, 1f);
        var mid = ShapeKey.For(MapShape.Rectangle, 0.5f, 1f);
        var round = ShapeKey.For(MapShape.Rectangle, 1f, 1f);
        var wide = ShapeKey.For(MapShape.Rectangle, 0f, 2f);
        Assert.NotEqual(sharp, mid);
        Assert.NotEqual(mid, round);
        Assert.NotEqual(sharp, wide);
    }

    [Fact]
    public void Oval_IgnoresCornerRadius()
    {
        Assert.Equal(ShapeKey.For(MapShape.Oval, 0f, 1f), ShapeKey.For(MapShape.Oval, 1f, 1f));
    }
}
