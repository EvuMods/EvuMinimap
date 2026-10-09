using System;

namespace EvuMinimap.Core;

public static class ShapeKey
{
    public static int For(MapShape shape, float cornerRadius, float aspect)
    {
        var radiusKey = shape == MapShape.Oval
            ? 100
            : (int)Math.Round(AppearanceMath.ClampCornerRadius(cornerRadius) * 100f);
        var aspectKey = (int)Math.Round(AppearanceMath.ClampAspect(aspect) * 100f);
        // radiusKey is 0..100 and aspectKey is 50..200. A million per shape keeps
        // those ranges from landing on another shape. 100000 did not: oval's forced
        // radius of 100 collided with a sharp rectangle.
        return ((int)shape * 1_000_000) + (radiusKey * 1000) + aspectKey;
    }
}
