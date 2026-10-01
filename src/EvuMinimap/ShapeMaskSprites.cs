using System;
using System.Collections.Generic;
using EvuMinimap.Core;
using UnityEngine;

namespace EvuMinimap;

internal static class ShapeMaskSprites
{
    const int Size = 128;
    static readonly Dictionary<int, Sprite> Cache = new Dictionary<int, Sprite>();

    public static Sprite Get(MapShape shape, float cornerRadius, float aspect)
    {
        var radiusKey = shape == MapShape.Oval
            ? 100
            : (int)Math.Round(AppearanceMath.ClampCornerRadius(cornerRadius) * 100f);
        var aspectKey = (int)Math.Round(AppearanceMath.ClampAspect(aspect) * 100f);
        var key = ((int)shape * 100000) + (radiusKey * 1000) + aspectKey;
        if (Cache.TryGetValue(key, out var existing))
        {
            return existing;
        }

        var sprite = Create(shape, radiusKey / 100f, aspectKey / 100f);
        Cache[key] = sprite;
        return sprite;
    }

    static Sprite Create(MapShape shape, float radius, float aspect)
    {
        var window = AppearanceMath.ClipWindowFor(aspect);
        var left = window.X * Size;
        var bottom = window.Y * Size;
        var width = window.Width * Size;
        var height = window.Height * Size;
        var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        var pixels = new Color32[Size * Size];
        var limit = radius * 0.5f * Math.Min(width, height);
        var centerX = left + (width * 0.5f);
        var centerY = bottom + (height * 0.5f);
        var radiusX = width * 0.5f;
        var radiusY = height * 0.5f;
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                var px = x + 0.5f;
                var py = y + 0.5f;
                var inside = shape == MapShape.Oval
                    ? InsideEllipse(px, py, centerX, centerY, radiusX, radiusY)
                    : InsideRoundedRect(px, py, left, bottom, width, height, limit);
                pixels[(y * Size) + x] = new Color32(255, 255, 255, inside ? (byte)255 : (byte)0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f), 100f);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    static bool InsideEllipse(float px, float py, float centerX, float centerY, float radiusX, float radiusY)
    {
        if (radiusX <= 0f || radiusY <= 0f)
        {
            return false;
        }

        var nx = (px - centerX) / radiusX;
        var ny = (py - centerY) / radiusY;
        return (nx * nx) + (ny * ny) <= 1f;
    }

    static bool InsideRoundedRect(float px, float py, float left, float bottom, float width, float height, float limit)
    {
        var lx = px - left;
        var ly = py - bottom;
        if (lx < 0f || ly < 0f || lx > width || ly > height)
        {
            return false;
        }

        var cx = lx < limit ? limit : (lx > width - limit ? width - limit : lx);
        var cy = ly < limit ? limit : (ly > height - limit ? height - limit : ly);
        var dx = lx - cx;
        var dy = ly - cy;
        return (dx * dx) + (dy * dy) <= (limit * limit);
    }
}
