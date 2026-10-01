namespace EvuMinimap.Core;

/// <summary>
/// Saved minimap settings. Five of these are stored. The solver takes one.
/// </summary>
public readonly struct MinimapProfile
{
    public static MinimapProfile Vanilla { get; } = new MinimapProfile(MapAnchor.TopRight, 0f, 0f, 1f, true);

    public MinimapProfile(MapAnchor anchor, float offsetX, float offsetY, float scale, bool repositionBuffs)
        : this(anchor, offsetX, offsetY, scale, repositionBuffs, MapShape.None, 1f, 0f, 1f)
    {
    }

    public MinimapProfile(
        MapAnchor anchor,
        float offsetX,
        float offsetY,
        float scale,
        bool repositionBuffs,
        MapShape shape,
        float alpha,
        float cornerRadius,
        float aspect)
    {
        Anchor = anchor;
        OffsetX = offsetX;
        OffsetY = offsetY;
        Scale = ScaleMath.Clamp(scale);
        RepositionBuffs = repositionBuffs;
        Shape = shape;
        Alpha = AppearanceMath.ClampAlpha(alpha);
        CornerRadius = AppearanceMath.ClampCornerRadius(cornerRadius);
        Aspect = AppearanceMath.ClampAspect(aspect);
    }

    public MapAnchor Anchor { get; }

    public float OffsetX { get; }

    public float OffsetY { get; }

    public float Scale { get; }

    public bool RepositionBuffs { get; }

    public MapShape Shape { get; }

    public float Alpha { get; }

    public float CornerRadius { get; }

    public float Aspect { get; }

    public MinimapProfile WithScale(float scale)
    {
        return new MinimapProfile(Anchor, OffsetX, OffsetY, scale, RepositionBuffs, Shape, Alpha, CornerRadius, Aspect);
    }
}
