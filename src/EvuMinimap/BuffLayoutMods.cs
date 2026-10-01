using BepInEx.Bootstrap;

namespace EvuMinimap;

internal static class BuffLayoutMods
{
    static readonly string[] Ids =
    {
        "randyknapp.mods.minimalstatuseffects",
        "redseiko.valheim.statusquo",
    };

    public static bool BlocksBuffMove(out string mod)
    {
        var infos = Chainloader.PluginInfos;
        for (var i = 0; i < Ids.Length; i++)
        {
            if (infos.ContainsKey(Ids[i]))
            {
                mod = Ids[i];
                return true;
            }
        }

        mod = string.Empty;
        return false;
    }
}
