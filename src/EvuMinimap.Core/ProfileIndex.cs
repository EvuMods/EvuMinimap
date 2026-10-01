namespace EvuMinimap.Core;

public static class ProfileIndex
{
    public const int Count = 5;

    public static int Clamp(int index)
    {
        if (index < 1)
        {
            return 1;
        }

        if (index > Count)
        {
            return Count;
        }

        return index;
    }

    public static int Cycle(int active, int direction)
    {
        var index = Clamp(active) - 1;
        var step = direction < 0 ? -1 : 1;
        var wrapped = (index + step) % Count;
        if (wrapped < 0)
        {
            wrapped += Count;
        }

        return wrapped + 1;
    }
}
