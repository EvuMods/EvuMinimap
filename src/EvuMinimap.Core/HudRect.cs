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
        return X - margin < other.Right
            && Right + margin > other.X
            && Y - margin < other.Top
            && Top + margin > other.Y;
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
