using EvuMinimap.Core;
using UnityEngine;

namespace EvuMinimap;

internal sealed class MinimapApplier
{
    readonly PluginConfig _config;
    readonly BepInEx.Logging.ManualLogSource _log;
    bool _checkedConflicts;
    bool _moveBuffs = true;
    bool _capturedMap;
    VanillaLayout _vanillaMap;
    RectFields _vanillaBuffs;
    int _buffId;

    public MinimapApplier(PluginConfig config, BepInEx.Logging.ManualLogSource log)
    {
        _config = config;
        _log = log;
    }

    public void Tick()
    {
        NoteConflicts();

        if (Game.m_noMap)
        {
            RestoreBuffs();
            return;
        }

        var minimap = Minimap.instance;
        if (minimap == null || minimap.m_smallRoot == null)
        {
            return;
        }

        var small = minimap.m_smallRoot.GetComponent<RectTransform>();
        if (small == null)
        {
            return;
        }

        var showSmall = minimap.m_smallRoot.activeInHierarchy && minimap.m_mode == Minimap.MapMode.Small;
        if (!showSmall)
        {
            RestoreBuffs();
            return;
        }

        if (!_capturedMap)
        {
            _vanillaMap = ReadVanilla(small);
            _capturedMap = true;
        }

        var parent = small.parent as RectTransform;
        if (parent == null)
        {
            return;
        }

        var profile = _config.Current;
        var layout = LayoutSolver.Solve(_vanillaMap, ReadParent(parent), profile);
        Apply(small, layout);

        var hud = Hud.instance;
        var buffs = hud != null ? hud.m_statusEffectListRoot : null;
        if (buffs == null || !_moveBuffs || !profile.RepositionBuffs)
        {
            RestoreBuffs();
            return;
        }

        PlaceBuffs(parent, layout, buffs);
    }

    void NoteConflicts()
    {
        if (_checkedConflicts)
        {
            return;
        }

        _checkedConflicts = true;
        if (BuffLayoutMods.BlocksBuffMove(out var mod))
        {
            _moveBuffs = false;
            _log.LogInfo("Leaving buff icons alone because " + mod + " is loaded.");
        }
    }

    void PlaceBuffs(RectTransform mapParent, RectLayout layout, RectTransform buffs)
    {
        var buffParent = buffs.parent as RectTransform;
        var canvas = CanvasOf(mapParent);
        if (buffParent == null || canvas == null)
        {
            return;
        }

        if (_buffId != buffs.GetInstanceID())
        {
            _vanillaBuffs = ReadFields(buffs);
            _buffId = buffs.GetInstanceID();
        }

        var mapBounds = LayoutSolver.Bounds(ReadParent(mapParent), layout);
        var mapCanvas = ToSpace(mapBounds, mapParent, canvas);
        var vanillaBounds = LayoutSolver.Bounds(ReadParent(buffParent), _vanillaBuffs);
        var vanillaCanvas = ToSpace(vanillaBounds, buffParent, canvas);
        var desiredCanvas = BuffPlacement.Place(mapCanvas, vanillaCanvas, ReadParent(canvas), reposition: true);
        var desiredParent = ToSpace(desiredCanvas, canvas, buffParent);
        WriteBuff(buffs, _vanillaBuffs, desiredParent.X - vanillaBounds.X, desiredParent.Y - vanillaBounds.Y);
    }

    void RestoreBuffs()
    {
        if (_buffId == 0)
        {
            return;
        }

        var hud = Hud.instance;
        var buffs = hud != null ? hud.m_statusEffectListRoot : null;
        if (buffs == null || buffs.GetInstanceID() != _buffId)
        {
            return;
        }

        WriteBuff(buffs, _vanillaBuffs, 0f, 0f);
    }

    static VanillaLayout ReadVanilla(RectTransform rect)
    {
        var fields = ReadFields(rect);
        var pointAnchor = Nearly(fields.AnchorMinX, fields.AnchorMaxX) && Nearly(fields.AnchorMinY, fields.AnchorMaxY);
        if (pointAnchor)
        {
            return new VanillaLayout(
                fields.AnchorMinX,
                fields.AnchorMinY,
                fields.PivotX,
                fields.PivotY,
                fields.AnchoredX,
                fields.AnchoredY,
                fields.Width,
                fields.Height,
                fields.ScaleX,
                fields.ScaleY);
        }

        var parent = rect.parent as RectTransform;
        if (parent == null)
        {
            return new VanillaLayout(0.5f, 0.5f, fields.PivotX, fields.PivotY, fields.AnchoredX, fields.AnchoredY, fields.Width, fields.Height, fields.ScaleX, fields.ScaleY);
        }

        var bounds = LayoutSolver.Bounds(ReadParent(parent), fields);
        var scaleX = Nearly(fields.ScaleX, 0f) ? 1f : fields.ScaleX;
        var scaleY = Nearly(fields.ScaleY, 0f) ? 1f : fields.ScaleY;
        var parentRect = ReadParent(parent);
        parentRect.AnchorReference(fields.PivotX, fields.PivotY, out var referenceX, out var referenceY);
        var pivotX = bounds.X + (bounds.Width * fields.PivotX);
        var pivotY = bounds.Y + (bounds.Height * fields.PivotY);
        return new VanillaLayout(
            fields.PivotX,
            fields.PivotY,
            fields.PivotX,
            fields.PivotY,
            pivotX - referenceX,
            pivotY - referenceY,
            bounds.Width / scaleX,
            bounds.Height / scaleY,
            fields.ScaleX,
            fields.ScaleY);
    }

    static void Apply(RectTransform rect, RectLayout layout)
    {
        if (Matches(rect, layout))
        {
            return;
        }

        var anchor = new Vector2(layout.AnchorX, layout.AnchorY);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.sizeDelta = new Vector2(layout.Width, layout.Height);
        var scale = rect.localScale;
        rect.localScale = new Vector3(layout.ScaleX, layout.ScaleY, scale.z);
        rect.anchoredPosition = new Vector2(layout.AnchoredX, layout.AnchoredY);
    }

    static void WriteBuff(RectTransform rect, RectFields saved, float deltaX, float deltaY)
    {
        var anchoredX = saved.AnchoredX + deltaX;
        var anchoredY = saved.AnchoredY + deltaY;
        if (Matches(rect, saved, anchoredX, anchoredY))
        {
            return;
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchorMin = new Vector2(saved.AnchorMinX, saved.AnchorMinY);
        rect.anchorMax = new Vector2(saved.AnchorMaxX, saved.AnchorMaxY);
        rect.pivot = new Vector2(saved.PivotX, saved.PivotY);
        rect.sizeDelta = new Vector2(saved.Width, saved.Height);
        var scale = rect.localScale;
        rect.localScale = new Vector3(saved.ScaleX, saved.ScaleY, scale.z);
        rect.anchoredPosition = new Vector2(anchoredX, anchoredY);
    }

    static bool Matches(RectTransform rect, RectLayout layout)
    {
        return Nearly(rect.anchorMin.x, layout.AnchorX)
            && Nearly(rect.anchorMin.y, layout.AnchorY)
            && Nearly(rect.anchorMax.x, layout.AnchorX)
            && Nearly(rect.anchorMax.y, layout.AnchorY)
            && Nearly(rect.pivot.x, layout.PivotX)
            && Nearly(rect.pivot.y, layout.PivotY)
            && Nearly(rect.anchoredPosition.x, layout.AnchoredX)
            && Nearly(rect.anchoredPosition.y, layout.AnchoredY)
            && Nearly(rect.sizeDelta.x, layout.Width)
            && Nearly(rect.sizeDelta.y, layout.Height)
            && Nearly(rect.localScale.x, layout.ScaleX)
            && Nearly(rect.localScale.y, layout.ScaleY);
    }

    static bool Matches(RectTransform rect, RectFields saved, float anchoredX, float anchoredY)
    {
        return Nearly(rect.anchorMin.x, saved.AnchorMinX)
            && Nearly(rect.anchorMin.y, saved.AnchorMinY)
            && Nearly(rect.anchorMax.x, saved.AnchorMaxX)
            && Nearly(rect.anchorMax.y, saved.AnchorMaxY)
            && Nearly(rect.pivot.x, saved.PivotX)
            && Nearly(rect.pivot.y, saved.PivotY)
            && Nearly(rect.anchoredPosition.x, anchoredX)
            && Nearly(rect.anchoredPosition.y, anchoredY)
            && Nearly(rect.sizeDelta.x, saved.Width)
            && Nearly(rect.sizeDelta.y, saved.Height)
            && Nearly(rect.localScale.x, saved.ScaleX)
            && Nearly(rect.localScale.y, saved.ScaleY);
    }

    static RectFields ReadFields(RectTransform rect)
    {
        var scale = rect.localScale;
        return new RectFields(
            rect.anchorMin.x,
            rect.anchorMin.y,
            rect.anchorMax.x,
            rect.anchorMax.y,
            rect.pivot.x,
            rect.pivot.y,
            rect.anchoredPosition.x,
            rect.anchoredPosition.y,
            rect.sizeDelta.x,
            rect.sizeDelta.y,
            scale.x,
            scale.y);
    }

    static ParentRect ReadParent(RectTransform parent)
    {
        var rect = parent.rect;
        return new ParentRect(rect.x, rect.y, rect.width, rect.height);
    }

    static RectTransform? CanvasOf(RectTransform rect)
    {
        var canvas = rect.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return rect.parent as RectTransform;
        }

        var root = canvas.rootCanvas != null ? canvas.rootCanvas.transform : canvas.transform;
        return root as RectTransform;
    }

    static HudRect ToSpace(HudRect rect, RectTransform from, RectTransform to)
    {
        var minimum = to.InverseTransformPoint(from.TransformPoint(new Vector3(rect.X, rect.Y, 0f)));
        var maximum = to.InverseTransformPoint(from.TransformPoint(new Vector3(rect.Right, rect.Top, 0f)));
        return new HudRect(minimum.x, minimum.y, maximum.x - minimum.x, maximum.y - minimum.y);
    }

    static bool Nearly(float left, float right)
    {
        var delta = left - right;
        if (delta < 0f)
        {
            delta = -delta;
        }

        return delta <= 0.01f;
    }
}
