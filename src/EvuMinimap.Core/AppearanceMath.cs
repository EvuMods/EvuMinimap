using System;

namespace EvuMinimap.Core;

public static class AppearanceMath
{
    public const float MinAspect = 0.5f;
    public const float MaxAspect = 2f;

    public static float ClampAlpha(float alpha)
    {
        if (float.IsNaN(alpha) || alpha < 0f)
        {
            return 0f;
        }

        if (alpha > 1f)
        {
            return 1f;
        }

        return alpha;
    }

    public static float ClampCornerRadius(float radius)
    {
        if (float.IsNaN(radius) || radius < 0f)
        {
            return 0f;
        }

        if (radius > 1f)
        {
            return 1f;
        }

        return radius;
    }

    public static float ClampAspect(float aspect)
    {
        if (float.IsNaN(aspect) || aspect < MinAspect)
        {
            return MinAspect;
        }

        if (aspect > MaxAspect)
        {
            return MaxAspect;
        }

        return aspect;
    }

    /// <summary>
    /// Largest window inside the map, in 0–1 map space, with the requested width/height.
    /// The map itself stays square. A wide aspect crops the top and bottom.
    /// </summary>
    public static ClipWindow ClipWindowFor(float aspect)
    {
        aspect = ClampAspect(aspect);
        if (aspect >= 1f)
        {
            var height = 1f / aspect;
            return new ClipWindow(0f, (1f - height) * 0.5f, 1f, height);
        }

        var width = aspect;
        return new ClipWindow((1f - width) * 0.5f, 0f, width, 1f);
    }
}

public readonly struct ClipWindow
{
    public ClipWindow(float x, float y, float width, float height)
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
}
