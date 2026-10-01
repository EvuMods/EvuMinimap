using System;

namespace EvuMinimap.Core;

/// <summary>
/// Which point of the minimap stays fixed when the size changes.
/// Top-right grows down-left. Bottom-left grows up-right. Center grows outward.
/// </summary>
public enum MapAnchor
{
    TopLeft,
    Top,
    TopRight,
    Left,
    Center,
    Right,
    BottomLeft,
    Bottom,
    BottomRight,
}

public static class MapAnchorMath
{
    public static void ToFractions(this MapAnchor anchor, out float x, out float y)
    {
        switch (anchor)
        {
            case MapAnchor.TopLeft:
                x = 0f;
                y = 1f;
                return;
            case MapAnchor.Top:
                x = 0.5f;
                y = 1f;
                return;
            case MapAnchor.TopRight:
                x = 1f;
                y = 1f;
                return;
            case MapAnchor.Left:
                x = 0f;
                y = 0.5f;
                return;
            case MapAnchor.Center:
                x = 0.5f;
                y = 0.5f;
                return;
            case MapAnchor.Right:
                x = 1f;
                y = 0.5f;
                return;
            case MapAnchor.BottomLeft:
                x = 0f;
                y = 0f;
                return;
            case MapAnchor.Bottom:
                x = 0.5f;
                y = 0f;
                return;
            case MapAnchor.BottomRight:
                x = 1f;
                y = 0f;
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(anchor), anchor, "Unknown map anchor.");
        }
    }
}
