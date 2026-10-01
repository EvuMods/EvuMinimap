namespace EvuMinimap.Core;

public static class BuffPlacement
{
    public const float Gap = 8f;

    public static HudRect Place(HudRect map, HudRect vanillaBuffs, ParentRect parent, bool reposition)
    {
        if (!reposition || !map.Intersects(vanillaBuffs, Gap))
        {
            return vanillaBuffs;
        }

        var below = Slide(vanillaBuffs, vanillaBuffs.X, map.Y - Gap - vanillaBuffs.Height);
        if (Fits(below, map, parent))
        {
            return below;
        }

        var above = Slide(vanillaBuffs, vanillaBuffs.X, map.Top + Gap);
        var left = Slide(vanillaBuffs, map.X - Gap - vanillaBuffs.Width, vanillaBuffs.Y);
        var right = Slide(vanillaBuffs, map.Right + Gap, vanillaBuffs.Y);

        if (TryBestFit(map, parent, above, left, right, out var fitted))
        {
            return fitted;
        }

        return Roomiest(map, vanillaBuffs, parent);
    }

    static bool TryBestFit(HudRect map, ParentRect parent, HudRect above, HudRect left, HudRect right, out HudRect placed)
    {
        placed = default;
        var found = false;
        var bestSpace = float.NegativeInfinity;
        Consider(above, FreeAbove(map, parent), map, parent, ref found, ref bestSpace, ref placed);
        Consider(left, FreeLeft(map, parent), map, parent, ref found, ref bestSpace, ref placed);
        Consider(right, FreeRight(map, parent), map, parent, ref found, ref bestSpace, ref placed);
        return found;
    }

    static void Consider(
        HudRect candidate,
        float space,
        HudRect map,
        ParentRect parent,
        ref bool found,
        ref float bestSpace,
        ref HudRect placed)
    {
        if (!Fits(candidate, map, parent) || space < bestSpace)
        {
            return;
        }

        found = true;
        bestSpace = space;
        placed = candidate;
    }

    static HudRect Roomiest(HudRect map, HudRect vanillaBuffs, ParentRect parent)
    {
        var belowSpace = FreeBelow(map, parent);
        var aboveSpace = FreeAbove(map, parent);
        var leftSpace = FreeLeft(map, parent);
        var rightSpace = FreeRight(map, parent);
        var best = belowSpace;
        var side = Side.Below;
        if (aboveSpace > best)
        {
            best = aboveSpace;
            side = Side.Above;
        }

        if (leftSpace > best)
        {
            best = leftSpace;
            side = Side.Left;
        }

        if (rightSpace > best)
        {
            side = Side.Right;
        }

        var width = vanillaBuffs.Width;
        var height = vanillaBuffs.Height;
        float x;
        float y;
        switch (side)
        {
            case Side.Above:
                x = map.X + ((map.Width - width) * 0.5f);
                y = map.Top + Gap;
                break;
            case Side.Left:
                x = map.X - Gap - width;
                y = map.Y + ((map.Height - height) * 0.5f);
                break;
            case Side.Right:
                x = map.Right + Gap;
                y = map.Y + ((map.Height - height) * 0.5f);
                break;
            default:
                x = map.X + ((map.Width - width) * 0.5f);
                y = map.Y - Gap - height;
                break;
        }

        return Clamp(new HudRect(x, y, width, height), parent);
    }

    static HudRect Clamp(HudRect rect, ParentRect parent)
    {
        var x = rect.X;
        var y = rect.Y;
        if (rect.Width <= parent.Width)
        {
            if (x < parent.X)
            {
                x = parent.X;
            }

            if (x + rect.Width > parent.Right)
            {
                x = parent.Right - rect.Width;
            }
        }
        else
        {
            x = parent.X;
        }

        if (rect.Height <= parent.Height)
        {
            if (y < parent.Y)
            {
                y = parent.Y;
            }

            if (y + rect.Height > parent.Top)
            {
                y = parent.Top - rect.Height;
            }
        }
        else
        {
            y = parent.Y;
        }

        return new HudRect(x, y, rect.Width, rect.Height);
    }

    static HudRect Slide(HudRect buff, float x, float y)
    {
        return new HudRect(x, y, buff.Width, buff.Height);
    }

    static bool Fits(HudRect candidate, HudRect map, ParentRect parent)
    {
        return candidate.FitsInside(parent) && !candidate.Intersects(map, Gap);
    }

    static float FreeBelow(HudRect map, ParentRect parent)
    {
        return map.Y - parent.Y;
    }

    static float FreeAbove(HudRect map, ParentRect parent)
    {
        return parent.Top - map.Top;
    }

    static float FreeLeft(HudRect map, ParentRect parent)
    {
        return map.X - parent.X;
    }

    static float FreeRight(HudRect map, ParentRect parent)
    {
        return parent.Right - map.Right;
    }

    enum Side
    {
        Below,
        Above,
        Left,
        Right,
    }
}
