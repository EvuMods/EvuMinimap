using System;
using System.IO;

namespace EvuMinimap;

internal static class ServerGuard
{
    public static bool IsDedicatedServer
    {
        get
        {
            var args = Environment.GetCommandLineArgs();
            if (args == null || args.Length == 0 || string.IsNullOrEmpty(args[0]))
            {
                return false;
            }

            var name = Path.GetFileNameWithoutExtension(args[0]);
            return name.IndexOf("valheim_server", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("valheim-server", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
