using System;

namespace EvuMinimap.Core;

public static class ScaleMath
{
    public const float Min = 0.5f;
    public const float Max = 5f;
    public const float DefaultStep = 0.25f;

    public static float Clamp(float scale)
    {
        if (float.IsNaN(scale) || scale < Min)
        {
            return Min;
        }

        if (scale > Max)
        {
            return Max;
        }

        return scale;
    }

    public static float Step(float scale, float step, int direction)
    {
        var amount = Math.Abs(step);
        if (amount <= 0f || float.IsNaN(amount))
        {
            amount = DefaultStep;
        }

        var sign = direction < 0 ? -1f : 1f;
        return Clamp(scale + (sign * amount));
    }
}
