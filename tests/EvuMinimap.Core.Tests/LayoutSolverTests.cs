using EvuMinimap.Core;
using Xunit;

namespace EvuMinimap.Core.Tests;

public sealed class LayoutSolverTests
{
    static readonly ParentRect Parent = new ParentRect(0f, 0f, 1920f, 1080f);

    static readonly VanillaLayout Vanilla = new VanillaLayout(
        anchorX: 1f,
        anchorY: 1f,
        pivotX: 1f,
        pivotY: 1f,
        anchoredX: -20f,
        anchoredY: -20f,
        width: 200f,
        height: 200f,
        scaleX: 1f,
        scaleY: 1f);

    static readonly HudRect VanillaBounds = new HudRect(1700f, 860f, 200f, 200f);

    [Fact]
    public void ScaleOne_TopRight_ZeroOffset_MatchesVanilla()
    {
        var layout = Solve(MinimapProfile.Vanilla);
        var bounds = LayoutSolver.Bounds(Parent, layout);

        AssertNear(VanillaBounds, bounds);
        Assert.Equal(-20f, layout.AnchoredX, 3);
        Assert.Equal(-20f, layout.AnchoredY, 3);
        Assert.Equal(1f, layout.ScaleX, 3);
    }

    [Theory]
    [InlineData(MapAnchor.TopLeft)]
    [InlineData(MapAnchor.Top)]
    [InlineData(MapAnchor.TopRight)]
    [InlineData(MapAnchor.Left)]
    [InlineData(MapAnchor.Center)]
    [InlineData(MapAnchor.Right)]
    [InlineData(MapAnchor.BottomLeft)]
    [InlineData(MapAnchor.Bottom)]
    [InlineData(MapAnchor.BottomRight)]
    public void ChangingAnchorAtScaleOne_DoesNotMoveTheMap(MapAnchor anchor)
    {
        var profile = new MinimapProfile(anchor, 0f, 0f, 1f, true);
        var bounds = LayoutSolver.Bounds(Parent, Solve(profile));
        AssertNear(VanillaBounds, bounds);
    }

    [Fact]
    public void ScaleTwo_TopRight_GrowsDownLeft()
    {
        var bounds = Bounds(MapAnchor.TopRight, 0f, 0f, 2f);
        AssertNear(new HudRect(1500f, 660f, 400f, 400f), bounds);
    }

    [Fact]
    public void ScaleTwo_BottomLeft_GrowsUpRight()
    {
        var bounds = Bounds(MapAnchor.BottomLeft, 0f, 0f, 2f);
        AssertNear(new HudRect(1700f, 860f, 400f, 400f), bounds);
    }

    [Fact]
    public void ScaleTwo_Center_GrowsOutward()
    {
        var bounds = Bounds(MapAnchor.Center, 0f, 0f, 2f);
        AssertNear(new HudRect(1600f, 760f, 400f, 400f), bounds);
    }

    [Fact]
    public void ScaleTwo_Top_KeepsTheTopEdge()
    {
        var bounds = Bounds(MapAnchor.Top, 0f, 0f, 2f);
        AssertNear(new HudRect(1600f, 660f, 400f, 400f), bounds);
    }

    [Fact]
    public void Offset_MovesTheAnchorFromVanilla()
    {
        var bounds = Bounds(MapAnchor.TopRight, -10f, -20f, 1f);
        AssertNear(new HudRect(1690f, 840f, 200f, 200f), bounds);
    }

    [Fact]
    public void Offset_OnBottomLeft_MovesThatCorner()
    {
        var bounds = Bounds(MapAnchor.BottomLeft, 10f, 20f, 1f);
        AssertNear(new HudRect(1710f, 880f, 200f, 200f), bounds);
    }

    [Fact]
    public void Scale_IsClamped()
    {
        var bounds = Bounds(MapAnchor.TopRight, 0f, 0f, 9f);
        AssertNear(new HudRect(900f, 60f, 1000f, 1000f), bounds);
    }

    [Fact]
    public void VanillaScale_IsMultiplied()
    {
        var vanilla = new VanillaLayout(1f, 1f, 1f, 1f, -20f, -20f, 200f, 200f, 1.5f, 1.5f);
        var profile = new MinimapProfile(MapAnchor.TopRight, 0f, 0f, 2f, true);
        var layout = LayoutSolver.Solve(vanilla, Parent, profile);
        Assert.Equal(3f, layout.ScaleX, 3);
        Assert.Equal(3f, layout.ScaleY, 3);
    }

    [Fact]
    public void ResetProfile_RestoresVanilla()
    {
        var profile = MinimapProfile.Vanilla;
        Assert.Equal(MapAnchor.TopRight, profile.Anchor);
        Assert.Equal(0f, profile.OffsetX);
        Assert.Equal(0f, profile.OffsetY);
        Assert.Equal(1f, profile.Scale);
        Assert.True(profile.RepositionBuffs);
        AssertNear(VanillaBounds, LayoutSolver.Bounds(Parent, Solve(profile)));
    }

    [Fact]
    public void SplitAnchors_UseTheAnchorCenterAsReference()
    {
        var fields = new RectFields(
            anchorMinX: 0f,
            anchorMinY: 0f,
            anchorMaxX: 1f,
            anchorMaxY: 1f,
            pivotX: 0.5f,
            pivotY: 0.5f,
            anchoredX: 0f,
            anchoredY: 0f,
            width: 0f,
            height: 0f,
            scaleX: 1f,
            scaleY: 1f);

        var bounds = LayoutSolver.Bounds(Parent, fields);
        AssertNear(new HudRect(0f, 0f, 1920f, 1080f), bounds);
    }

    static RectLayout Solve(MinimapProfile profile)
    {
        return LayoutSolver.Solve(Vanilla, Parent, profile);
    }

    static HudRect Bounds(MapAnchor anchor, float offsetX, float offsetY, float scale)
    {
        return LayoutSolver.Bounds(Parent, Solve(new MinimapProfile(anchor, offsetX, offsetY, scale, true)));
    }

    static void AssertNear(HudRect expected, HudRect actual)
    {
        Assert.Equal(expected.X, actual.X, 2);
        Assert.Equal(expected.Y, actual.Y, 2);
        Assert.Equal(expected.Width, actual.Width, 2);
        Assert.Equal(expected.Height, actual.Height, 2);
    }
}
