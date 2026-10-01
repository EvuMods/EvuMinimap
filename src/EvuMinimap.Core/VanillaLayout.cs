namespace EvuMinimap.Core;

/// <summary>
/// Point-anchor snapshot of the vanilla minimap, captured before this mod moves it.
/// </summary>
public readonly struct VanillaLayout
{
    public VanillaLayout(
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
}
