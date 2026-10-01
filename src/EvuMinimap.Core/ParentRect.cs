namespace EvuMinimap.Core;

/// <summary>
/// Parent rectangle in that transform's local space. X and Y are the minimum corner.
/// </summary>
public readonly struct ParentRect
{
    public ParentRect(float x, float y, float width, float height)
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

    public void AnchorReference(float anchorX, float anchorY, out float x, out float y)
    {
        x = X + (Width * anchorX);
        y = Y + (Height * anchorY);
    }
}
