namespace EvuMinimap.Core;

/// <summary>
/// General rect fields, including split anchors. Used to recompute the vanilla buff strip
/// after the screen size changes without baking a pixel snapshot.
/// </summary>
public readonly struct RectFields
{
    public RectFields(
        float anchorMinX,
        float anchorMinY,
        float anchorMaxX,
        float anchorMaxY,
        float pivotX,
        float pivotY,
        float anchoredX,
        float anchoredY,
        float width,
        float height,
        float scaleX,
        float scaleY)
    {
        AnchorMinX = anchorMinX;
        AnchorMinY = anchorMinY;
        AnchorMaxX = anchorMaxX;
        AnchorMaxY = anchorMaxY;
        PivotX = pivotX;
        PivotY = pivotY;
        AnchoredX = anchoredX;
        AnchoredY = anchoredY;
        Width = width;
        Height = height;
        ScaleX = scaleX;
        ScaleY = scaleY;
    }

    public float AnchorMinX { get; }

    public float AnchorMinY { get; }

    public float AnchorMaxX { get; }

    public float AnchorMaxY { get; }

    public float PivotX { get; }

    public float PivotY { get; }

    public float AnchoredX { get; }

    public float AnchoredY { get; }

    public float Width { get; }

    public float Height { get; }

    public float ScaleX { get; }

    public float ScaleY { get; }
}
