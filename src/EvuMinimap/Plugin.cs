using BepInEx;

namespace EvuMinimap;

[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
[BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    PluginConfig? _settings;
    MinimapApplier? _applier;

    void Awake()
    {
        if (ServerGuard.IsDedicatedServer)
        {
            Logger.LogInfo("Dedicated server detected. EvuMinimap stays inactive.");
            return;
        }

        _settings = new PluginConfig(Config, Logger);
        _applier = new MinimapApplier(_settings, Logger);
    }

    void Update()
    {
        _settings?.PollHotkeys();
    }

    void LateUpdate()
    {
        _applier?.Tick();
    }
}
