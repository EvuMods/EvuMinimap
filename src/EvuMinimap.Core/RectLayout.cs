namespace EvuMinimap.Core;

/// <summary>
/// Full point-anchor rect state to write onto the small-map root.
/// Width and height are the unscaled size. Scale multiplies them around the pivot.
/// </summary>
public readonly struct RectLayout
{
    public RectLayout(
        float anchorX,
        float anchorY,
        float pivotX,
        float pivotY,
        float anchoredX,
        float anchoredY,
        float width,
        float height,
        float scaleX,
        float scaleY)
    {
        AnchorX = anchorX;
        AnchorY = anchorY;
        PivotX = pivotX;
        PivotY = pivotY;
        AnchoredX = anchoredX;
        AnchoredY = anchoredY;
        Width = width;
        Height = height;
        ScaleX = scaleX;
        ScaleY = scaleY;
    }

    public float AnchorX { get; }

    public float AnchorY { get; }

    public float PivotX { get; }

    public float PivotY { get; }

    public float AnchoredX { get; }

    public float AnchoredY { get; }

    public float Width { get; }

    public float Height { get; }

    public float ScaleX { get; }

    public float ScaleY { get; }

    public bool Approximately(RectLayout other, float epsilon = 0.01f)
    {
        return Near(AnchorX, other.AnchorX, epsilon)
            && Near(AnchorY, other.AnchorY, epsilon)
            && Near(PivotX, other.PivotX, epsilon)
            && Near(PivotY, other.PivotY, epsilon)
            && Near(AnchoredX, other.AnchoredX, epsilon)
            && Near(AnchoredY, other.AnchoredY, epsilon)
            && Near(Width, other.Width, epsilon)
            && Near(Height, other.Height, epsilon)
            && Near(ScaleX, other.ScaleX, epsilon)
            && Near(ScaleY, other.ScaleY, epsilon);
    }

    static bool Near(float a, float b, float epsilon)
    {
        var delta = a - b;
        if (delta < 0f)
        {
            delta = -delta;
        }

        return delta <= epsilon;
    }
}
