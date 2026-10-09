using System;
using System.IO;
using BepInEx.Configuration;
using BepInEx.Logging;
using EvuMinimap.Core;
using UnityEngine;

namespace EvuMinimap;

internal sealed class PluginConfig
{
    readonly ProfileSlot[] _slots;
    readonly ConfigEntry<int> _active;
    readonly ConfigEntry<float> _scaleStep;
    readonly ConfigEntry<KeyboardShortcut> _increase;
    readonly ConfigEntry<KeyboardShortcut> _decrease;
    readonly ConfigEntry<KeyboardShortcut> _nextProfile;
    readonly ConfigEntry<KeyboardShortcut> _previousProfile;
    readonly ConfigEntry<bool> _reset;
    readonly ConfigEntry<bool> _enabled;
    readonly ManualLogSource _log;
    bool _updating;

    public PluginConfig(ConfigFile config, ManualLogSource log)
    {
        _log = log;
        MigrateShapeNames(config);
        _enabled = config.Bind(
            "General",
            "Enabled",
            true,
            new ConfigDescription(
                "When off, the small minimap, buff strip, and ship wind panel stay vanilla. Saved profiles and hotkeys remain in this file and apply again when this is on.",
                null,
                new ConfigurationManagerAttributes { Order = 100 }));
        _slots = new ProfileSlot[ProfileIndex.Count];
        for (var i = 0; i < _slots.Length; i++)
        {
            _slots[i] = ProfileSlot.Bind(config, i, ClampFloat);
        }

        _active = config.Bind(
            "Profiles",
            "Active",
            1,
            new ConfigDescription(
                "Which saved minimap profile is shown. Alt+Numpad multiply and divide cycle it.",
                new AcceptableValueRange<int>(1, ProfileIndex.Count),
                new ConfigurationManagerAttributes { Order = 100 }));

        _scaleStep = config.Bind(
            "Minimap",
            "ScaleStep",
            ScaleMath.DefaultStep,
            new ConfigDescription(
                "How much the size hotkeys change the scale of the active profile.",
                new AcceptableValueRange<float>(0.05f, 1f),
                new ConfigurationManagerAttributes { Order = 10, IsAdvanced = true }));

        _increase = config.Bind(
            "Hotkeys",
            "IncreaseSize",
            new KeyboardShortcut(KeyCode.KeypadPlus, KeyCode.LeftAlt),
            new ConfigDescription(
                "Increase the active profile's scale by one step. Either Alt key works.",
                null,
                new ConfigurationManagerAttributes { Order = 40 }));

        _decrease = config.Bind(
            "Hotkeys",
            "DecreaseSize",
            new KeyboardShortcut(KeyCode.KeypadMinus, KeyCode.LeftAlt),
            new ConfigDescription(
                "Decrease the active profile's scale by one step. Either Alt key works.",
                null,
                new ConfigurationManagerAttributes { Order = 30 }));

        _nextProfile = config.Bind(
            "Hotkeys",
            "NextProfile",
            new KeyboardShortcut(KeyCode.KeypadMultiply, KeyCode.LeftAlt),
            new ConfigDescription(
                "Switch to the next minimap profile. Either Alt key works.",
                null,
                new ConfigurationManagerAttributes { Order = 20 }));

        _previousProfile = config.Bind(
            "Hotkeys",
            "PreviousProfile",
            new KeyboardShortcut(KeyCode.KeypadDivide, KeyCode.LeftAlt),
            new ConfigDescription(
                "Switch to the previous minimap profile. Either Alt key works.",
                null,
                new ConfigurationManagerAttributes { Order = 10 }));

        _reset = config.Bind(
            "Actions",
            "ResetToVanilla",
            false,
            new ConfigDescription(
                "Restore the active profile: scale, anchor, offset, buff reposition, ship wind, shape mask, and icon alpha. Hotkeys and the other profiles are kept.",
                null,
                new ConfigurationManagerAttributes
                {
                    Order = 100,
                    HideDefaultButton = true,
                    HideSettingName = true,
                    CustomDrawer = DrawReset,
                }));

        QuietClamp(_active, ProfileIndex.Clamp);
        QuietClamp(_scaleStep, ClampStepValue);
        _active.SettingChanged += (_, __) => OnActiveChanged();
        _scaleStep.SettingChanged += (_, __) => QuietClamp(_scaleStep, ClampStepValue);
        _reset.SettingChanged += (_, __) =>
        {
            if (_reset.Value)
            {
                ApplyVanilla();
            }
        };
    }

    public MinimapProfile Current => ActiveSlot().Read();

    public bool Enabled => _enabled.Value;

    public void PollHotkeys()
    {
        if (!_enabled.Value || InputFocused())
        {
            return;
        }

        if (HotkeyInput.WasPressed(_increase.Value))
        {
            SetScale(ScaleMath.Step(ActiveSlot().Scale.Value, _scaleStep.Value, 1));
        }
        else if (HotkeyInput.WasPressed(_decrease.Value))
        {
            SetScale(ScaleMath.Step(ActiveSlot().Scale.Value, _scaleStep.Value, -1));
        }
        else if (HotkeyInput.WasPressed(_nextProfile.Value))
        {
            SetActive(ProfileIndex.Cycle(_active.Value, 1));
        }
        else if (HotkeyInput.WasPressed(_previousProfile.Value))
        {
            SetActive(ProfileIndex.Cycle(_active.Value, -1));
        }
    }

    static bool InputFocused()
    {
        if (Console.IsVisible())
        {
            return true;
        }

        var chat = Chat.instance;
        if (chat != null && chat.HasFocus())
        {
            return true;
        }

        return TextInput.IsVisible();
    }

    public void ApplyVanilla()
    {
        if (_updating)
        {
            return;
        }

        _updating = true;
        try
        {
            ActiveSlot().Write(MinimapProfile.Vanilla);
            if (_reset.Value)
            {
                _reset.Value = false;
            }
        }
        finally
        {
            _updating = false;
        }
    }

    void DrawReset(ConfigEntryBase entry)
    {
        if (GUILayout.Button("Reset to vanilla", GUILayout.ExpandWidth(true)))
        {
            ApplyVanilla();
        }
    }

    void SetScale(float scale)
    {
        var entry = ActiveSlot().Scale;
        if (Math.Abs(entry.Value - scale) < 0.0001f)
        {
            return;
        }

        entry.Value = scale;
    }

    void SetActive(int index)
    {
        index = ProfileIndex.Clamp(index);
        if (_active.Value == index)
        {
            return;
        }

        _active.Value = index;
    }

    void OnActiveChanged()
    {
        if (_updating)
        {
            return;
        }

        var index = ProfileIndex.Clamp(_active.Value);
        if (index != _active.Value)
        {
            _updating = true;
            try
            {
                _active.Value = index;
            }
            finally
            {
                _updating = false;
            }
        }

        if (!_enabled.Value)
        {
            return;
        }

        var message = "Switched to Minimap Profile " + index.ToString();
        _log.LogInfo(message);
        var hud = MessageHud.instance;
        if (hud != null)
        {
            hud.ShowMessage(MessageHud.MessageType.TopLeft, message);
        }
    }

    ProfileSlot ActiveSlot()
    {
        return _slots[ProfileIndex.Clamp(_active.Value) - 1];
    }

    void QuietClamp(ConfigEntry<int> entry, Func<int, int> clamp)
    {
        if (_updating)
        {
            return;
        }

        var clamped = clamp(entry.Value);
        if (clamped == entry.Value)
        {
            return;
        }

        _updating = true;
        try
        {
            entry.Value = clamped;
        }
        finally
        {
            _updating = false;
        }
    }

    void QuietClamp(ConfigEntry<float> entry, Func<float, float> clamp)
    {
        if (_updating)
        {
            return;
        }

        var clamped = clamp(entry.Value);
        if (Math.Abs(clamped - entry.Value) < 0.0001f)
        {
            return;
        }

        _updating = true;
        try
        {
            entry.Value = clamped;
        }
        finally
        {
            _updating = false;
        }
    }

    void ClampFloat(ConfigEntry<float> entry, Func<float, float> clamp)
    {
        entry.SettingChanged += (_, __) => QuietClamp(entry, clamp);
    }

    static float ClampStepValue(float step)
    {
        if (float.IsNaN(step) || step < 0.05f)
        {
            return 0.05f;
        }

        if (step > 1f)
        {
            return 1f;
        }

        return step;
    }

    static void MigrateShapeNames(ConfigFile config)
    {
        // BepInEx already has one converter for every enum, and GetConverter never
        // consults a per-type converter for them. AddConverter only logs a warning.
        var path = config.ConfigFilePath;
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return;
        }

        var text = File.ReadAllText(path);
        var updated = text
            .Replace("ShapeMask = Circle", "ShapeMask = Oval")
            .Replace("ShapeMask = Square", "ShapeMask = Rectangle");
        if (updated == text)
        {
            return;
        }

        File.WriteAllText(path, updated);
        config.Reload();
    }

    sealed class ProfileSlot
    {
        readonly ConfigEntry<float> _scale;
        readonly ConfigEntry<MapAnchor> _anchor;
        readonly ConfigEntry<float> _offsetX;
        readonly ConfigEntry<float> _offsetY;
        readonly ConfigEntry<bool> _repositionBuffs;
        readonly ConfigEntry<bool> _repositionShipHud;
        readonly ConfigEntry<MapShape> _shape;
        readonly ConfigEntry<float> _alpha;
        readonly ConfigEntry<float> _cornerRadius;
        readonly ConfigEntry<float> _aspect;
        readonly ConfigurationManagerAttributes _aspectAttributes;
        readonly ConfigurationManagerAttributes _cornerAttributes;

        ProfileSlot(
            ConfigEntry<float> scale,
            ConfigEntry<MapAnchor> anchor,
            ConfigEntry<float> offsetX,
            ConfigEntry<float> offsetY,
            ConfigEntry<bool> repositionBuffs,
            ConfigEntry<bool> repositionShipHud,
            ConfigEntry<MapShape> shape,
            ConfigEntry<float> alpha,
            ConfigEntry<float> cornerRadius,
            ConfigEntry<float> aspect,
            ConfigurationManagerAttributes aspectAttributes,
            ConfigurationManagerAttributes cornerAttributes)
        {
            _scale = scale;
            _anchor = anchor;
            _offsetX = offsetX;
            _offsetY = offsetY;
            _repositionBuffs = repositionBuffs;
            _repositionShipHud = repositionShipHud;
            _shape = shape;
            _alpha = alpha;
            _cornerRadius = cornerRadius;
            _aspect = aspect;
            _aspectAttributes = aspectAttributes;
            _cornerAttributes = cornerAttributes;
            _shape.SettingChanged += OnShapeChanged;
            UpdateLocks();
        }

        void OnShapeChanged(object sender, EventArgs args)
        {
            UpdateLocks();
        }

        void UpdateLocks()
        {
            var unlocked = _shape.Value != MapShape.None;
            _aspectAttributes.ReadOnly = !unlocked;
            _cornerAttributes.ReadOnly = !unlocked || _shape.Value == MapShape.Oval;
        }

        public ConfigEntry<float> Scale => _scale;

        public static ProfileSlot Bind(ConfigFile config, int index, Action<ConfigEntry<float>, Func<float, float>> clamp)
        {
            var section = index == 0 ? "Minimap" : "Minimap " + (index + 1).ToString();
            var shape = config.Bind(
                section,
                "ShapeMask",
                MapShape.None,
                new ConfigDescription(
                    "Extra clip on the small map. None keeps Valheim's shape, including changes from other mods. Oval is round, and aspect can stretch it. Rectangle uses corner radius.",
                    null,
                    new ConfigurationManagerAttributes { Order = 100, DispName = "Shape mask" }));
            var alphaDefault = 1f;
            var hadLegacyAlpha = config.TryGetEntry<float>(section, "Alpha", out var legacyAlpha);
            if (hadLegacyAlpha)
            {
                alphaDefault = legacyAlpha.Value;
            }

            var alpha = config.Bind(
                section,
                "IconAlpha",
                alphaDefault,
                new ConfigDescription(
                    "Opacity of the frame, pins, and markers. Does not fade the terrain.",
                    new AcceptableValueRange<float>(0f, 1f),
                    new ConfigurationManagerAttributes { Order = 96, DispName = "Icon alpha" }));
            if (hadLegacyAlpha)
            {
                config.Remove(legacyAlpha.Definition);
                config.Save();
            }
            var scale = config.Bind(
                section,
                "Scale",
                1f,
                new ConfigDescription(
                    "Minimap size relative to vanilla. 1 is the original size.",
                    new AcceptableValueRange<float>(ScaleMath.Min, ScaleMath.Max),
                    new ConfigurationManagerAttributes { Order = 90 }));
            var anchor = config.Bind(
                section,
                "Anchor",
                MapAnchor.TopRight,
                new ConfigDescription(
                    "Point of the minimap that stays in place when the size changes. Top-right grows down-left.",
                    null,
                    new ConfigurationManagerAttributes { Order = 80 }));
            var offsetX = config.Bind(
                section,
                "OffsetX",
                0f,
                new ConfigDescription(
                    "Moves the anchor horizontally, in HUD units, from its vanilla position.",
                    new AcceptableValueRange<float>(-4000f, 4000f),
                    new ConfigurationManagerAttributes { Order = 70 }));
            var offsetY = config.Bind(
                section,
                "OffsetY",
                0f,
                new ConfigDescription(
                    "Moves the anchor vertically, in HUD units, from its vanilla position.",
                    new AcceptableValueRange<float>(-4000f, 4000f),
                    new ConfigurationManagerAttributes { Order = 60 }));
            var repositionBuffs = config.Bind(
                section,
                "RepositionBuffIcons",
                true,
                new ConfigDescription(
                    "Slide buff icons off the minimap when they overlap it. Icons stay put when the map does not cover them.",
                    null,
                    new ConfigurationManagerAttributes { Order = 50 }));
            var repositionShipHud = config.Bind(
                section,
                "RepositionShipHud",
                true,
                new ConfigDescription(
                    "Slide the boat wind panel below the minimap when they overlap, if that spot fits on screen. Otherwise it moves to the side with the most room. It stays put when the map does not cover it.",
                    null,
                    new ConfigurationManagerAttributes { Order = 45, DispName = "Move ship wind" }));
            var aspectAttributes = new ConfigurationManagerAttributes { Order = 40 };
            var cornerAttributes = new ConfigurationManagerAttributes { Order = 30 };
            var aspect = config.Bind(
                section,
                "Aspect",
                1f,
                new ConfigDescription(
                    "Width relative to height of the shape mask. The map and icons stay 1:1. Above 1 hides more of the top and bottom. Below 1 hides more of the sides. Locked while shape mask is None.",
                    new AcceptableValueRange<float>(AppearanceMath.MinAspect, AppearanceMath.MaxAspect),
                    aspectAttributes));
            var cornerRadius = config.Bind(
                section,
                "CornerRadius",
                0f,
                new ConfigDescription(
                    "Corner roundness of the rectangle mask. 0 is sharp. 1 is a capsule. Locked for None and oval.",
                    new AcceptableValueRange<float>(0f, 1f),
                    cornerAttributes));

            clamp(scale, ScaleMath.Clamp);
            clamp(alpha, AppearanceMath.ClampAlpha);
            clamp(aspect, AppearanceMath.ClampAspect);
            clamp(cornerRadius, AppearanceMath.ClampCornerRadius);
            return new ProfileSlot(scale, anchor, offsetX, offsetY, repositionBuffs, repositionShipHud, shape, alpha, cornerRadius, aspect, aspectAttributes, cornerAttributes);
        }

        public MinimapProfile Read()
        {
            return new MinimapProfile(
                _anchor.Value,
                _offsetX.Value,
                _offsetY.Value,
                _scale.Value,
                _repositionBuffs.Value,
                _shape.Value,
                _alpha.Value,
                _cornerRadius.Value,
                _aspect.Value,
                _repositionShipHud.Value);
        }

        public void Write(MinimapProfile profile)
        {
            _scale.Value = profile.Scale;
            _anchor.Value = profile.Anchor;
            _offsetX.Value = profile.OffsetX;
            _offsetY.Value = profile.OffsetY;
            _repositionBuffs.Value = profile.RepositionBuffs;
            _repositionShipHud.Value = profile.RepositionShipHud;
            _shape.Value = profile.Shape;
            _alpha.Value = profile.Alpha;
            _cornerRadius.Value = profile.CornerRadius;
            _aspect.Value = profile.Aspect;
        }
    }
}
