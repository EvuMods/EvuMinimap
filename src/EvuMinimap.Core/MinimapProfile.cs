namespace EvuMinimap.Core;

/// <summary>
/// Saved minimap settings. One profile today. Later profiles are more of these.
/// </summary>
public readonly struct MinimapProfile
{
    public static MinimapProfile Vanilla { get; } = new MinimapProfile(MapAnchor.TopRight, 0f, 0f, 1f, true);

    public MinimapProfile(MapAnchor anchor, float offsetX, float offsetY, float scale, bool repositionBuffs)
    {
        Anchor = anchor;
        OffsetX = offsetX;
        OffsetY = offsetY;
        Scale = ScaleMath.Clamp(scale);
        RepositionBuffs = repositionBuffs;
    }

    public MapAnchor Anchor { get; }

    public float OffsetX { get; }

    public float OffsetY { get; }

    public float Scale { get; }

    public bool RepositionBuffs { get; }

    public MinimapProfile WithScale(float scale)
    {
        return new MinimapProfile(Anchor, OffsetX, OffsetY, scale, RepositionBuffs);
    }
}
