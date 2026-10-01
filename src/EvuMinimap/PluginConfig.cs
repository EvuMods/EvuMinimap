using System;
using BepInEx.Configuration;
using EvuMinimap.Core;
using UnityEngine;

namespace EvuMinimap;

internal sealed class PluginConfig
{
    readonly ConfigEntry<float> _scale;
    readonly ConfigEntry<MapAnchor> _anchor;
    readonly ConfigEntry<float> _offsetX;
    readonly ConfigEntry<float> _offsetY;
    readonly ConfigEntry<bool> _repositionBuffs;
    readonly ConfigEntry<float> _scaleStep;
    readonly ConfigEntry<KeyboardShortcut> _increase;
    readonly ConfigEntry<KeyboardShortcut> _decrease;
    readonly ConfigEntry<bool> _reset;
    bool _updating;

    public PluginConfig(ConfigFile config)
    {
        _scale = config.Bind(
            "Minimap",
            "Scale",
            1f,
            new ConfigDescription(
                "Minimap size relative to vanilla. 1 is the original size.",
                new AcceptableValueRange<float>(ScaleMath.Min, ScaleMath.Max),
                new ConfigurationManagerAttributes { Order = 90 }));

        _anchor = config.Bind(
            "Minimap",
            "Anchor",
            MapAnchor.TopRight,
            new ConfigDescription(
                "Point of the minimap that stays in place when the size changes. Top-right grows down-left.",
                null,
                new ConfigurationManagerAttributes { Order = 80 }));

        _offsetX = config.Bind(
            "Minimap",
            "OffsetX",
            0f,
            new ConfigDescription(
                "Moves the anchor horizontally, in HUD units, from its vanilla position.",
                new AcceptableValueRange<float>(-4000f, 4000f),
                new ConfigurationManagerAttributes { Order = 70 }));

        _offsetY = config.Bind(
            "Minimap",
            "OffsetY",
            0f,
            new ConfigDescription(
                "Moves the anchor vertically, in HUD units, from its vanilla position.",
                new AcceptableValueRange<float>(-4000f, 4000f),
                new ConfigurationManagerAttributes { Order = 60 }));

        _repositionBuffs = config.Bind(
            "Minimap",
            "RepositionBuffIcons",
            true,
            new ConfigDescription(
                "Slide buff icons off the minimap when they overlap it. Icons stay put when the map does not cover them.",
                null,
                new ConfigurationManagerAttributes { Order = 50 }));

        _scaleStep = config.Bind(
            "Minimap",
            "ScaleStep",
            ScaleMath.DefaultStep,
            new ConfigDescription(
                "How much the size hotkeys change the scale.",
                new AcceptableValueRange<float>(0.05f, 1f),
                new ConfigurationManagerAttributes { Order = 10, IsAdvanced = true }));

        _increase = config.Bind(
            "Hotkeys",
            "IncreaseSize",
            new KeyboardShortcut(KeyCode.KeypadPlus, KeyCode.LeftAlt),
            new ConfigDescription(
                "Increase the minimap scale by one step. Either Alt key works.",
                null,
                new ConfigurationManagerAttributes { Order = 20 }));

        _decrease = config.Bind(
            "Hotkeys",
            "DecreaseSize",
            new KeyboardShortcut(KeyCode.KeypadMinus, KeyCode.LeftAlt),
            new ConfigDescription(
                "Decrease the minimap scale by one step. Either Alt key works.",
                null,
                new ConfigurationManagerAttributes { Order = 10 }));

        _reset = config.Bind(
            "Actions",
            "ResetToVanilla",
            false,
            new ConfigDescription(
                "Restore scale, anchor, offset, and buff reposition. Hotkeys are kept.",
                null,
                new ConfigurationManagerAttributes
                {
                    Order = 100,
                    HideDefaultButton = true,
                    HideSettingName = true,
                    CustomDrawer = DrawReset,
                }));

        _scale.SettingChanged += (_, __) => ClampScale();
        _scaleStep.SettingChanged += (_, __) => ClampStep();
        _reset.SettingChanged += (_, __) =>
        {
            if (_reset.Value)
            {
                ApplyVanilla();
            }
        };
    }

    public MinimapProfile Current =>
        new MinimapProfile(_anchor.Value, _offsetX.Value, _offsetY.Value, _scale.Value, _repositionBuffs.Value);

    public void PollHotkeys()
    {
        if (HotkeyInput.WasPressed(_increase.Value))
        {
            SetScale(ScaleMath.Step(_scale.Value, _scaleStep.Value, 1));
        }
        else if (HotkeyInput.WasPressed(_decrease.Value))
        {
            SetScale(ScaleMath.Step(_scale.Value, _scaleStep.Value, -1));
        }
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
            var vanilla = MinimapProfile.Vanilla;
            _scale.Value = vanilla.Scale;
            _anchor.Value = vanilla.Anchor;
            _offsetX.Value = vanilla.OffsetX;
            _offsetY.Value = vanilla.OffsetY;
            _repositionBuffs.Value = vanilla.RepositionBuffs;
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
        if (Math.Abs(_scale.Value - scale) < 0.0001f)
        {
            return;
        }

        _scale.Value = scale;
    }

    void ClampScale()
    {
        if (_updating)
        {
            return;
        }

        var clamped = ScaleMath.Clamp(_scale.Value);
        if (Math.Abs(clamped - _scale.Value) < 0.0001f)
        {
            return;
        }

        _updating = true;
        try
        {
            _scale.Value = clamped;
        }
        finally
        {
            _updating = false;
        }
    }

    void ClampStep()
    {
        if (_updating)
        {
            return;
        }

        var step = _scaleStep.Value;
        if (float.IsNaN(step) || step < 0.05f)
        {
            step = 0.05f;
        }
        else if (step > 1f)
        {
            step = 1f;
        }

        if (Math.Abs(step - _scaleStep.Value) < 0.0001f)
        {
            return;
        }

        _updating = true;
        try
        {
            _scaleStep.Value = step;
        }
        finally
        {
            _updating = false;
        }
    }
}
