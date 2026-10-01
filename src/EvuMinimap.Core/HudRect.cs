namespace EvuMinimap.Core;

/// <summary>
/// Axis-aligned rectangle. X and Y are the minimum corner (bottom-left in UI space).
/// </summary>
public readonly struct HudRect
{
    public HudRect(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public float X { get; }

    public float Y { get; }

    public float Width { get; }

    public float Height { get; }

    public float Right => X + Width;

    public float Top => Y + Height;

    public bool Intersects(HudRect other, float margin)
    {
        // A slot placed exactly one gap away can still test as overlapping after
        // float rounding. A fraction of a pixel does not cover the map.
        const float slack = 0.5f;
        return X - margin < other.Right - slack
            && Right + margin > other.X + slack
            && Y - margin < other.Top - slack
            && Top + margin > other.Y + slack;
    }

    public bool FitsInside(ParentRect parent)
    {
        const float edge = 0.01f;
        return X >= parent.X - edge
            && Y >= parent.Y - edge
            && Right <= parent.Right + edge
            && Top <= parent.Top + edge;
    }
}
