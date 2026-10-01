using System.Collections.Generic;
using EvuMinimap.Core;
using UnityEngine;
using UnityEngine.UI;

namespace EvuMinimap;

internal sealed class MinimapApplier
{
    readonly PluginConfig _config;
    readonly BepInEx.Logging.ManualLogSource _log;
    bool _checkedConflicts;
    bool _moveBuffs = true;
    bool _capturedMap;
    const float BuffVisualPad = 18f;
    GameObject? _clip;
    Mask? _mask;
    Image? _maskImage;
    bool _maskableSaved;
    bool _savedMaskable;
    bool _clipHooked;
    CanvasGroup? _canvasGroup;
    bool _ownsCanvasGroup;
    bool _savedCanvasAlphaKnown;
    float _savedCanvasAlpha;
    bool _touched;
    readonly List<Graphic> _tinted = new List<Graphic>();
    readonly List<Color> _tintBase = new List<Color>();
    readonly List<Graphic> _tintSeen = new List<Graphic>();
    VanillaLayout _vanillaMap;
    RectFields _vanillaBuffs;
    int _buffId;
    RectFields _vanillaShip;
    int _shipId;
    int _shipIconId;
    Vector2 _shipIconVanilla;

    public MinimapApplier(PluginConfig config, BepInEx.Logging.ManualLogSource log)
    {
        _config = config;
        _log = log;
    }

    public void Tick()
    {
        if (!_config.Enabled)
        {
            ReleaseToVanilla();
            return;
        }

        NoteConflicts();

        if (Game.m_noMap)
        {
            RestoreBuffs();
            RestoreShip();
            RestoreCustomTints();
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
            RestoreShip();
            RestoreCustomTints();
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
        ApplyShape(minimap, small.gameObject, profile);
        ApplyAlpha(minimap, small.gameObject, profile.Alpha);
        _touched = true;

        var hud = Hud.instance;
        var buffs = hud != null ? hud.m_statusEffectListRoot : null;
        if (buffs == null || !_moveBuffs || !profile.RepositionBuffs)
        {
            RestoreBuffs();
        }
        else
        {
            PlacePiece(parent, layout, buffs, ref _vanillaBuffs, ref _buffId, ClearanceSide.Left);
        }

        var ship = ShipRect(hud);
        if (ship == null || !profile.RepositionShipHud)
        {
            RestoreShip();
        }
        else
        {
            PlacePiece(parent, layout, ship, ref _vanillaShip, ref _shipId, ClearanceSide.Below, padFirst: true);
            SyncShipIcon(hud, ship);
        }
    }

    void ReleaseToVanilla()
    {
        if (!_touched && !_capturedMap && _clip == null && _buffId == 0 && _shipId == 0 && _shipIconId == 0 && _canvasGroup == null && _tinted.Count == 0)
        {
            return;
        }

        var minimap = Minimap.instance;
        if (minimap == null || minimap.m_smallRoot == null)
        {
            return;
        }

        var root = minimap.m_smallRoot;
        var small = root.GetComponent<RectTransform>();
        if (small == null)
        {
            return;
        }

        if (_capturedMap)
        {
            WriteVanilla(small, _vanillaMap);
            _capturedMap = false;
        }

        ReleaseClip(root);
        UseLiveMapMaterial(minimap, clip: false);
        RestoreAlpha(root);
        RestoreCustomTints();
        _tinted.Clear();
        _tintBase.Clear();
        _tintSeen.Clear();
        if (!RestoreBuffs() || !RestoreShip())
        {
            return;
        }

        _buffId = 0;
        _shipId = 0;
        _shipIconId = 0;
        _touched = false;
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

    void ApplyShape(Minimap minimap, GameObject root, MinimapProfile profile)
    {
        if (profile.Shape == MapShape.None)
        {
            ReleaseClip(root);
            UseLiveMapMaterial(minimap, clip: false);
            return;
        }

        EnsureClip(root);
        if (_mask == null || _maskImage == null)
        {
            return;
        }

        if (!_mask.enabled)
        {
            _mask.enabled = true;
        }

        var sprite = ShapeMaskSprites.Get(profile.Shape, profile.CornerRadius, profile.Aspect);
        if (_maskImage.sprite != sprite)
        {
            _maskImage.sprite = sprite;
        }

        if (_maskImage.type != Image.Type.Simple)
        {
            _maskImage.type = Image.Type.Simple;
        }

        if (_maskImage.preserveAspect)
        {
            _maskImage.preserveAspect = false;
        }

        UseLiveMapMaterial(minimap, clip: true);
    }

    void EnsureClip(GameObject root)
    {
        if (_clip != null)
        {
            return;
        }

        var existing = new List<Transform>(root.transform.childCount);
        for (var i = 0; i < root.transform.childCount; i++)
        {
            existing.Add(root.transform.GetChild(i));
        }

        var clip = new GameObject("EvuMinimapClip");
        clip.layer = root.layer;
        var rect = clip.AddComponent<RectTransform>();
        rect.SetParent(root.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        for (var i = 0; i < existing.Count; i++)
        {
            existing[i].SetParent(rect, true);
        }

        var image = clip.AddComponent<Image>();
        image.raycastTarget = false;
        image.color = Color.white;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        var mask = clip.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        _clip = clip;
        _mask = mask;
        _maskImage = image;
    }

    void ReleaseClip(GameObject root)
    {
        if (_clip == null)
        {
            return;
        }

        var clip = _clip.transform;
        var children = new List<Transform>(clip.childCount);
        for (var i = 0; i < clip.childCount; i++)
        {
            children.Add(clip.GetChild(i));
        }

        for (var i = 0; i < children.Count; i++)
        {
            children[i].SetParent(root.transform, true);
        }

        UnityEngine.Object.Destroy(_clip);
        _clip = null;
        _mask = null;
        _maskImage = null;
        _clipHooked = false;
    }

    void UseLiveMapMaterial(Minimap minimap, bool clip)
    {
        var map = minimap.m_mapImageSmall;
        if (map == null)
        {
            return;
        }

        if (!_maskableSaved)
        {
            _savedMaskable = map.maskable;
            _maskableSaved = true;
        }

        if (!clip)
        {
            map.maskable = _savedMaskable;
            return;
        }

        if (!_clipHooked)
        {
            map.maskable = false;
            map.maskable = true;
            _clipHooked = true;
        }
        else if (!map.maskable)
        {
            map.maskable = true;
        }

        SyncMaskedMap(map, minimap.m_mapSmallShader);
    }

    static void SyncMaskedMap(RawImage map, Material source)
    {
        if (source == null)
        {
            return;
        }

        var rendered = map.materialForRendering;
        if (rendered == null || rendered == source)
        {
            return;
        }

        var hasStencil = rendered.HasProperty("_StencilComp");
        var stencil = hasStencil && rendered.HasProperty("_Stencil") ? rendered.GetInt("_Stencil") : 0;
        var stencilComp = hasStencil ? rendered.GetInt("_StencilComp") : 0;
        var stencilOp = rendered.HasProperty("_StencilOp") ? rendered.GetInt("_StencilOp") : 0;
        var stencilRead = rendered.HasProperty("_StencilReadMask") ? rendered.GetInt("_StencilReadMask") : 255;
        var stencilWrite = rendered.HasProperty("_StencilWriteMask") ? rendered.GetInt("_StencilWriteMask") : 255;
        var colorMask = rendered.HasProperty("_ColorMask") ? rendered.GetInt("_ColorMask") : 15;
        rendered.CopyPropertiesFromMaterial(source);
        if (!hasStencil)
        {
            return;
        }

        rendered.SetInt("_Stencil", stencil);
        rendered.SetInt("_StencilComp", stencilComp);
        if (rendered.HasProperty("_StencilOp"))
        {
            rendered.SetInt("_StencilOp", stencilOp);
        }

        if (rendered.HasProperty("_StencilReadMask"))
        {
            rendered.SetInt("_StencilReadMask", stencilRead);
        }

        if (rendered.HasProperty("_StencilWriteMask"))
        {
            rendered.SetInt("_StencilWriteMask", stencilWrite);
        }

        if (rendered.HasProperty("_ColorMask"))
        {
            rendered.SetInt("_ColorMask", colorMask);
        }
    }

    void ApplyAlpha(Minimap minimap, GameObject root, float alpha)
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = root.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = root.AddComponent<CanvasGroup>();
                _ownsCanvasGroup = true;
            }
            else if (!_savedCanvasAlphaKnown)
            {
                _savedCanvasAlpha = _canvasGroup.alpha;
                _savedCanvasAlphaKnown = true;
            }
        }

        if (!Nearly(_canvasGroup.alpha, alpha))
        {
            _canvasGroup.alpha = alpha;
        }

        TintCustomShaders(root, minimap.m_mapImageSmall, alpha);
    }

    void RestoreAlpha(GameObject root)
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = root.GetComponent<CanvasGroup>();
        }

        if (_canvasGroup == null)
        {
            return;
        }

        if (_ownsCanvasGroup)
        {
            UnityEngine.Object.Destroy(_canvasGroup);
            _canvasGroup = null;
            _ownsCanvasGroup = false;
            return;
        }

        if (_savedCanvasAlphaKnown && !Nearly(_canvasGroup.alpha, _savedCanvasAlpha))
        {
            _canvasGroup.alpha = _savedCanvasAlpha;
        }

        _canvasGroup = null;
    }

    void TintCustomShaders(GameObject root, RawImage? map, float alpha)
    {
        var graphics = root.GetComponentsInChildren<Graphic>(true);
        for (var i = 0; i < graphics.Length; i++)
        {
            var graphic = graphics[i];
            if (graphic == null || graphic == map || Seen(graphic))
            {
                continue;
            }

            _tintSeen.Add(graphic);
            var mat = graphic.material;
            if (mat == null || mat.shader == null || !mat.HasProperty("_Color"))
            {
                continue;
            }

            var shaderName = mat.shader.name;
            if (shaderName.StartsWith("UI/") || shaderName.StartsWith("TextMeshPro"))
            {
                continue;
            }

            var basis = mat.GetColor("_Color");
            var shared = BaseFor(mat);
            _tinted.Add(graphic);
            _tintBase.Add(shared >= 0 ? _tintBase[shared] : basis);
        }

        for (var i = 0; i < _tinted.Count; i++)
        {
            var graphic = _tinted[i];
            if (graphic == null)
            {
                continue;
            }

            var mat = graphic.material;
            if (mat == null || !mat.HasProperty("_Color"))
            {
                continue;
            }

            var color = _tintBase[i];
            color.a *= alpha;
            if (mat.GetColor("_Color") != color)
            {
                mat.SetColor("_Color", color);
            }
        }
    }

    void RestoreCustomTints()
    {
        for (var i = 0; i < _tinted.Count; i++)
        {
            var graphic = _tinted[i];
            if (graphic == null)
            {
                continue;
            }

            var mat = graphic.material;
            if (mat == null || !mat.HasProperty("_Color"))
            {
                continue;
            }

            if (mat.GetColor("_Color") != _tintBase[i])
            {
                mat.SetColor("_Color", _tintBase[i]);
            }
        }
    }

    bool Seen(Graphic graphic)
    {
        for (var i = 0; i < _tintSeen.Count; i++)
        {
            if (_tintSeen[i] == graphic)
            {
                return true;
            }
        }

        return false;
    }

    int BaseFor(Material mat)
    {
        for (var i = 0; i < _tinted.Count; i++)
        {
            var graphic = _tinted[i];
            if (graphic != null && graphic.material == mat)
            {
                return i;
            }
        }

        return -1;
    }

    static RectTransform? ShipRect(Hud? hud)
    {
        // The ring and boat sit on this rect, just under the minimap.
        // m_shipHudRoot is the rudder and sail cluster, and its rect does not meet the map.
        return hud != null ? hud.m_shipWindIndicatorRoot : null;
    }

    void PlacePiece(
        RectTransform mapParent,
        RectLayout layout,
        RectTransform piece,
        ref RectFields vanilla,
        ref int id,
        ClearanceSide prefer,
        bool padFirst = false)
    {
        var pieceParent = piece.parent as RectTransform;
        var canvas = CanvasOf(mapParent);
        if (pieceParent == null || canvas == null)
        {
            return;
        }

        if (id != piece.GetInstanceID())
        {
            vanilla = ReadFields(piece);
            id = piece.GetInstanceID();
        }

        var mapBounds = LayoutSolver.Bounds(ReadParent(mapParent), layout);
        var mapCanvas = ToSpace(mapBounds, mapParent, canvas);
        var blocking = padFirst ? Inflate(mapCanvas, BuffVisualPad) : mapCanvas;

        var vanillaBounds = LayoutSolver.Bounds(ReadParent(pieceParent), vanilla);
        var vanillaCanvas = ToSpace(vanillaBounds, pieceParent, canvas);
        var canvasSpace = ReadParent(canvas);
        var desiredCanvas = BuffPlacement.Place(mapCanvas, blocking, vanillaCanvas, canvasSpace, true, prefer);
        if (!padFirst && (!Nearly(desiredCanvas.X, vanillaCanvas.X) || !Nearly(desiredCanvas.Y, vanillaCanvas.Y)))
        {
            desiredCanvas = BuffPlacement.Place(Inflate(mapCanvas, BuffVisualPad), vanillaCanvas, canvasSpace, reposition: true, prefer);
        }

        var desiredParent = ToSpace(desiredCanvas, canvas, pieceParent);
        WriteBuff(piece, vanilla, desiredParent.X - vanillaBounds.X, desiredParent.Y - vanillaBounds.Y);
    }

    static HudRect Inflate(HudRect rect, float pad)
    {
        return new HudRect(rect.X - pad, rect.Y - pad, rect.Width + (pad * 2f), rect.Height + (pad * 2f));
    }

    void SyncShipIcon(Hud? hud, RectTransform indicator)
    {
        var icon = hud != null ? hud.m_shipWindIconRoot : null;
        if (icon == null || icon == indicator || icon.IsChildOf(indicator) || icon.parent != indicator.parent)
        {
            return;
        }

        if (_shipIconId != icon.GetInstanceID())
        {
            _shipIconVanilla = icon.anchoredPosition;
            _shipIconId = icon.GetInstanceID();
        }

        var target = _shipIconVanilla + (indicator.anchoredPosition - new Vector2(_vanillaShip.AnchoredX, _vanillaShip.AnchoredY));
        if (icon.anchoredPosition != target)
        {
            icon.anchoredPosition = target;
        }
    }

    void RestoreShipIcon()
    {
        if (_shipIconId == 0)
        {
            return;
        }

        var hud = Hud.instance;
        var icon = hud != null ? hud.m_shipWindIconRoot : null;
        if (icon == null || icon.GetInstanceID() != _shipIconId)
        {
            return;
        }

        if (icon.anchoredPosition != _shipIconVanilla)
        {
            icon.anchoredPosition = _shipIconVanilla;
        }
    }

    bool RestoreShip()
    {
        var restored = RestorePiece(_shipId, _vanillaShip, ShipRect(Hud.instance));
        if (restored)
        {
            RestoreShipIcon();
        }

        return restored;
    }

    bool RestoreBuffs()
    {
        var hud = Hud.instance;
        var buffs = hud != null ? hud.m_statusEffectListRoot : null;
        return RestorePiece(_buffId, _vanillaBuffs, buffs);
    }

    bool RestorePiece(int id, RectFields saved, RectTransform? piece)
    {
        if (id == 0)
        {
            return true;
        }

        if (piece == null)
        {
            return false;
        }

        if (piece.GetInstanceID() != id)
        {
            return true;
        }

        WriteBuff(piece, saved, 0f, 0f);
        return true;
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

    static void WriteVanilla(RectTransform rect, VanillaLayout layout)
    {
        if (Nearly(rect.anchorMin.x, layout.AnchorX)
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
            && Nearly(rect.localScale.y, layout.ScaleY))
        {
            return;
        }

        var anchor = new Vector2(layout.AnchorX, layout.AnchorY);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(layout.PivotX, layout.PivotY);
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
