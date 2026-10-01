using EvuMinimap.Core;
using Xunit;

namespace EvuMinimap.Core.Tests;

public sealed class BuffPlacementTests
{
    static readonly ParentRect Screen = new ParentRect(0f, 0f, 1920f, 1080f);

    [Fact]
    public void ClearMap_LeavesBuffsWhereValheimPutThem()
    {
        var map = new HudRect(0f, 0f, 200f, 200f);
        var buffs = new HudRect(1600f, 700f, 300f, 140f);
        var placed = BuffPlacement.Place(map, buffs, Screen, reposition: true);
        AssertSame(buffs, placed);
    }

    [Fact]
    public void Disabled_LeavesBuffsEvenWhenTheyOverlap()
    {
        var map = new HudRect(1500f, 660f, 400f, 400f);
        var buffs = new HudRect(1600f, 700f, 300f, 140f);
        var placed = BuffPlacement.Place(map, buffs, Screen, reposition: false);
        AssertSame(buffs, placed);
    }

    [Fact]
    public void Overlap_PrefersLeftOfTheMap()
    {
        var map = new HudRect(1500f, 660f, 400f, 400f);
        var buffs = new HudRect(1600f, 700f, 300f, 140f);
        var placed = BuffPlacement.Place(map, buffs, Screen, reposition: true);
        AssertSame(new HudRect(1192f, 700f, 300f, 140f), placed);
    }

    [Fact]
    public void Overlap_UsesTheRoomiestSideWhenLeftDoesNotFit()
    {
        var map = new HudRect(0f, 10f, 200f, 100f);
        var buffs = new HudRect(10f, 20f, 80f, 80f);
        var placed = BuffPlacement.Place(map, buffs, Screen, reposition: true);
        AssertSame(new HudRect(208f, 20f, 80f, 80f), placed);
    }

    [Fact]
    public void Overlap_UsesAboveWhenItHasMoreRoomThanBelow()
    {
        var map = new HudRect(0f, 400f, 1800f, 200f);
        var buffs = new HudRect(100f, 450f, 80f, 80f);
        var placed = BuffPlacement.Place(map, buffs, Screen, reposition: true);
        AssertSame(new HudRect(100f, 608f, 80f, 80f), placed);
    }

    [Fact]
    public void Overlap_UsesAboveWhenItIsTheOnlySideThatFits()
    {
        var map = new HudRect(10f, 10f, 1900f, 80f);
        var buffs = new HudRect(100f, 20f, 100f, 100f);
        var placed = BuffPlacement.Place(map, buffs, Screen, reposition: true);
        AssertSame(new HudRect(100f, 98f, 100f, 100f), placed);
    }

    [Fact]
    public void LeftWinsWhenItFits_EvenIfBelowHasMoreRoom()
    {
        var map = new HudRect(100f, 800f, 100f, 100f);
        var buffs = new HudRect(100f, 820f, 50f, 50f);
        var placed = BuffPlacement.Place(map, buffs, Screen, reposition: true);
        AssertSame(new HudRect(42f, 820f, 50f, 50f), placed);
    }

    [Fact]
    public void NoSideFits_ClampsOntoTheRoomiestSide()
    {
        var parent = new ParentRect(0f, 0f, 200f, 200f);
        var map = new HudRect(10f, 10f, 180f, 180f);
        var buffs = new HudRect(50f, 50f, 100f, 100f);
        var placed = BuffPlacement.Place(map, buffs, parent, reposition: true);
        AssertSame(new HudRect(0f, 50f, 100f, 100f), placed);
    }

    [Fact]
    public void Overlap_PrefersBelowTheMapForTheShipPanel()
    {
        var map = new HudRect(1500f, 700f, 400f, 300f);
        var panel = new HudRect(1550f, 640f, 250f, 80f);
        var placed = BuffPlacement.Place(map, panel, Screen, reposition: true, ClearanceSide.Below);
        AssertSame(new HudRect(1550f, 612f, 250f, 80f), placed);
    }

    [Fact]
    public void BelowPreference_MovesAsideWhenTheScreenHasNoRoomUnderTheMap()
    {
        var parent = new ParentRect(0f, 0f, 1920f, 1080f);
        var map = new HudRect(1500f, 20f, 400f, 1040f);
        var panel = new HudRect(1600f, 40f, 180f, 100f);
        var placed = BuffPlacement.Place(map, panel, parent, reposition: true, ClearanceSide.Below);
        AssertSame(new HudRect(1312f, 490f, 180f, 100f), placed);
    }

    [Fact]
    public void BelowPreference_UsesTheVisibleEdgeWhenThePadWouldFallOffScreen()
    {
        var parent = new ParentRect(0f, 0f, 1920f, 1080f);
        var map = new HudRect(1400f, 100f, 500f, 900f);
        var panel = new HudRect(1500f, 120f, 130f, 80f);
        var padded = new HudRect(map.X - 18f, map.Y - 18f, map.Width + 36f, map.Height + 36f);
        var placed = BuffPlacement.Place(map, padded, panel, parent, true, ClearanceSide.Below);
        AssertSame(new HudRect(1500f, 12f, 130f, 80f), placed);
    }

    [Fact]
    public void ScaleThree_Ultrawide_KeepsTheVanillaWindIndicatorBelowTheMap()
    {
        var parent = new ParentRect(-2560f, -720f, 5120f, 1440f);
        var vanillaMap = new VanillaLayout(1f, 1f, 1f, 1f, -40f, -40f, 200f, 200f, 1f, 1f);
        var wind = LayoutSolver.Bounds(
            parent,
            new RectLayout(1f, 1f, 0.5f, 0.5f, -146f, -437f, 130f, 130f, 1f, 1f));
        var profile = new MinimapProfile(MapAnchor.TopRight, 0f, 0f, 3f, true);
        var map = LayoutSolver.Bounds(parent, LayoutSolver.Solve(vanillaMap, parent, profile));
        var padded = new HudRect(map.X - 18f, map.Y - 18f, map.Width + 36f, map.Height + 36f);
        var placed = BuffPlacement.Place(map, padded, wind, parent, true, ClearanceSide.Below);

        Assert.Equal(wind.X, placed.X, 2);
        Assert.True(placed.X + 1f >= map.X);
        Assert.True(placed.Top <= map.Y);
    }

    [Fact]
    public void ClearMap_LeavesTheShipPanelWhereValheimPutIt()
    {
        var map = new HudRect(1500f, 700f, 400f, 300f);
        var panel = new HudRect(1550f, 500f, 250f, 80f);
        var placed = BuffPlacement.Place(map, panel, Screen, reposition: true, ClearanceSide.Below);
        AssertSame(panel, placed);
    }

    static void AssertSame(HudRect expected, HudRect actual)
    {
        Assert.Equal(expected.X, actual.X, 2);
        Assert.Equal(expected.Y, actual.Y, 2);
        Assert.Equal(expected.Width, actual.Width, 2);
        Assert.Equal(expected.Height, actual.Height, 2);
    }
}
