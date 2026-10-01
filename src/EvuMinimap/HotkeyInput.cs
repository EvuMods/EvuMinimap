using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace EvuMinimap;

internal static class HotkeyInput
{
    static readonly KeyCode[] WatchedModifiers =
    {
        KeyCode.LeftShift,
        KeyCode.RightShift,
        KeyCode.LeftControl,
        KeyCode.RightControl,
        KeyCode.LeftAlt,
        KeyCode.RightAlt,
        KeyCode.LeftCommand,
        KeyCode.RightCommand,
    };

    public static bool WasPressed(KeyboardShortcut shortcut)
    {
        if (shortcut.MainKey == KeyCode.None || !Input.GetKeyDown(shortcut.MainKey))
        {
            return false;
        }

        var modifiers = new List<KeyCode>(shortcut.Modifiers);
        for (var i = 0; i < modifiers.Count; i++)
        {
            if (!IsHeld(modifiers[i]))
            {
                return false;
            }
        }

        return !ExtraModifierHeld(modifiers);
    }

    static bool IsHeld(KeyCode key)
    {
        if (IsAlt(key))
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        return Input.GetKey(key);
    }

    static bool ExtraModifierHeld(IEnumerable<KeyCode> configured)
    {
        var altConfigured = false;
        foreach (var modifier in configured)
        {
            if (IsAlt(modifier))
            {
                altConfigured = true;
                break;
            }
        }

        for (var i = 0; i < WatchedModifiers.Length; i++)
        {
            var key = WatchedModifiers[i];
            if (!Input.GetKey(key) || Contains(configured, key))
            {
                continue;
            }

            if (altConfigured && IsAlt(key))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    static bool Contains(IEnumerable<KeyCode> keys, KeyCode key)
    {
        foreach (var candidate in keys)
        {
            if (candidate == key)
            {
                return true;
            }
        }

        return false;
    }

    static bool IsAlt(KeyCode key)
    {
        return key == KeyCode.LeftAlt || key == KeyCode.RightAlt;
    }
}
