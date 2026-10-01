namespace EvuMinimap.Core;

public static class LayoutSolver
{
    public static RectLayout Solve(VanillaLayout vanilla, ParentRect parent, MinimapProfile profile)
    {
        profile.Anchor.ToFractions(out var anchorX, out var anchorY);
        var vanillaBounds = Bounds(parent, ToLayout(vanilla));
        var pinnedX = vanillaBounds.X + (vanillaBounds.Width * anchorX) + profile.OffsetX;
        var pinnedY = vanillaBounds.Y + (vanillaBounds.Height * anchorY) + profile.OffsetY;
        parent.AnchorReference(anchorX, anchorY, out var referenceX, out var referenceY);

        return new RectLayout(
            anchorX,
            anchorY,
            anchorX,
            anchorY,
            pinnedX - referenceX,
            pinnedY - referenceY,
            vanilla.Width,
            vanilla.Height,
            vanilla.ScaleX * profile.Scale,
            vanilla.ScaleY * profile.Scale);
    }

    public static HudRect Bounds(ParentRect parent, RectLayout layout)
    {
        return Bounds(parent, new RectFields(
            layout.AnchorX,
            layout.AnchorY,
            layout.AnchorX,
            layout.AnchorY,
            layout.PivotX,
            layout.PivotY,
            layout.AnchoredX,
            layout.AnchoredY,
            layout.Width,
            layout.Height,
            layout.ScaleX,
            layout.ScaleY));
    }

    /// <summary>
    /// Anchor reference is the center of the anchor rectangle. Size is the anchor span plus size delta,
    /// then multiplied by local scale around the pivot.
    /// </summary>
    public static HudRect Bounds(ParentRect parent, RectFields fields)
    {
        var anchorMinX = parent.X + (parent.Width * fields.AnchorMinX);
        var anchorMinY = parent.Y + (parent.Height * fields.AnchorMinY);
        var anchorMaxX = parent.X + (parent.Width * fields.AnchorMaxX);
        var anchorMaxY = parent.Y + (parent.Height * fields.AnchorMaxY);
        var referenceX = (anchorMinX + anchorMaxX) * 0.5f;
        var referenceY = (anchorMinY + anchorMaxY) * 0.5f;
        var pivotX = referenceX + fields.AnchoredX;
        var pivotY = referenceY + fields.AnchoredY;
        var width = (anchorMaxX - anchorMinX + fields.Width) * fields.ScaleX;
        var height = (anchorMaxY - anchorMinY + fields.Height) * fields.ScaleY;
        return new HudRect(
            pivotX - (fields.PivotX * width),
            pivotY - (fields.PivotY * height),
            width,
            height);
    }

    static RectLayout ToLayout(VanillaLayout vanilla)
    {
        return new RectLayout(
            vanilla.AnchorX,
            vanilla.AnchorY,
            vanilla.PivotX,
            vanilla.PivotY,
            vanilla.AnchoredX,
            vanilla.AnchoredY,
            vanilla.Width,
            vanilla.Height,
            vanilla.ScaleX,
            vanilla.ScaleY);
    }
}
